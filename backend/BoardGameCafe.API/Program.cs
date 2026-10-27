using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using BoardGameCafe.API.Data;
using BoardGameCafe.API.Extensions;
using BoardGameCafe.API.Filters;
using BoardGameCafe.API.HangfireJobs;
using BoardGameCafe.API.Middlewares;
using BoardGameCafe.API.Services;
using BoardGameCafe.Application.Extensions;
using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Interfaces;
using BoardGameCafe.Infrastructure.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;

namespace BoardGameCafe.API;

/// <summary>
/// 应用程序主入口类
/// </summary>
public class Program
{
    /// <summary>
    /// 应用程序主入口方法
    /// </summary>
    /// <param name="args">命令行参数</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .CreateLogger();

        try
        {
            builder.Host.UseSerilog();

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.Services.AddInfrastructure(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOpts => sqlOpts.CommandTimeout(60))); // 命令执行超时 60 秒，避免高峰期偶发慢查询立即报错
            builder.Services.AddApplication();
            builder.Services.AddApiServices();

            // 注册操作日志自动清理后台服务（每天凌晨2点按类型差异化清理过期日志）
            builder.Services.AddHostedService<LogCleanupService>();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

            var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                // CORS 白名单可配置（Cors:AllowedOrigins），未配置时保持全放行以兼容现有部署
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                     ?? Array.Empty<string>();
                options.AddPolicy("CorsPolicy", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                    else
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                });
            });

            builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<AuthOperationLogFilter>();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BoardGameCafe API",
                    Version = "v1",
                    Description = "桌游咖啡厅管理系统API"
                });

                var xmlFileApi = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPathApi = Path.Combine(AppContext.BaseDirectory, xmlFileApi);
                if (File.Exists(xmlPathApi))
                {
                    c.IncludeXmlComments(xmlPathApi);
                }

                var xmlFileApp = "BoardGameCafe.Application.xml";
                var xmlPathApp = Path.Combine(AppContext.BaseDirectory, xmlFileApp);
                if (File.Exists(xmlPathApp))
                {
                    c.IncludeXmlComments(xmlPathApp);
                }

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT授权头，使用 Bearer 方案。格式：'Authorization: Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
            builder.Services.AddHangfireServer();

            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
            builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
            builder.Services.Configure<ClientRateLimitPolicies>(builder.Configuration.GetSection("ClientRateLimitPolicies"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await DbInitializer.Initialize(services);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "数据库初始化时发生错误");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseSerilogRequestLogging();

            // 安全响应头（防点击劫持 / MIME 嗅探 / 信息泄露）
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                await next();
            });

            app.UseCors("CorsPolicy");

            // Swagger 仅开发环境开放（生产隐藏接口文档）
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BoardGameCafe API v1");
                });
            }

            app.UseRouting();

            app.UseIpRateLimiting();
            app.UseClientRateLimiting();

            app.UseAuthentication();
            app.UseAuthorization();

            // SPA static files serving + fallback (must be before MapControllers)
            var webRootPath = Path.Combine(app.Environment.ContentRootPath, "www");
            if (Directory.Exists(webRootPath))
            {
                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    DefaultFileNames = new List<string> { "index.html" }
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(webRootPath),
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800");
                    }
                });
            }

            app.Use(async (context, next) =>
            {
                await next();

                var requestPath = context.Request.Path.Value ?? string.Empty;
                if (context.Response.StatusCode == 404 && !Path.HasExtension(requestPath))
                {
                    if (!requestPath.StartsWith("/api") &&
                        !requestPath.StartsWith("/swagger") &&
                        !requestPath.StartsWith("/hangfire"))
                    {
                        context.Request.Path = "/index.html";
                        context.Response.StatusCode = 200;
                        await next();
                    }
                }
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = app.Environment.IsDevelopment()
                    ? new IDashboardAuthorizationFilter[] { new AnonymousAuthFilter() }
                    : new IDashboardAuthorizationFilter[] { new HangfireAuthorizationFilter() }
            });

            RecurringJob.AddOrUpdate<BirthdayCouponJob>(
                "birthday-coupon-job",
                job => job.ExecuteAsync(),
                Cron.Daily(0, 0),
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                });

            app.MapControllers();

            app.MapGet("/", () => Results.Redirect("/swagger"))
               .ExcludeFromDescription()
               .WithDisplayName("Root Redirect to Swagger");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用程序启动失败");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

/// <summary>
/// Hangfire Dashboard 匿名授权过滤器（开发环境使用）
/// </summary>
public class AnonymousAuthFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// 授权检查
    /// </summary>
    /// <param name="context">Dashboard上下文</param>
    /// <returns>始终返回true允许访问</returns>
    public bool Authorize(DashboardContext context) => true;
}

/// <summary>
/// Hangfire Dashboard 授权过滤器（生产环境使用）
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// 授权检查
    /// </summary>
    /// <param name="context">Dashboard上下文</param>
    /// <returns>是否授权</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
