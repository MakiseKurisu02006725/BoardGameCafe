using System.Net;
using System.Text.Json;
using BoardGameCafe.Common;
using BoardGameCafe.Common.Exceptions;
using FluentValidation;
using Serilog;
using ValidationException = FluentValidation.ValidationException;

namespace BoardGameCafe.API.Middlewares;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    /// <summary>
    /// 请求委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 初始化全局异常处理中间件的新实例
    /// </summary>
    /// <param name="next">请求委托</param>
    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 执行中间件处理
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>任务</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 处理异常并返回统一格式响应
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="exception">捕获的异常</param>
    /// <returns>任务</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        ApiResponse response;

        switch (exception)
        {
            case BusinessException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Log.Warning(exception, "业务异常: {Message}", businessEx.Message);
                response = ApiResponse.Fail(businessEx.Message, businessEx.ErrorCode);
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                Log.Warning(exception, "资源未找到: {Message}", notFoundEx.Message);
                response = ApiResponse.Fail(notFoundEx.Message, 404);
                break;

            case UnauthorizedException unauthorizedEx:
                var isForbidden = unauthorizedEx.Message.Contains("权限不足");
                context.Response.StatusCode = isForbidden
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
                Log.Warning(exception, "未授权访问: {Message}", unauthorizedEx.Message);
                response = ApiResponse.Fail(unauthorizedEx.Message, context.Response.StatusCode);
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Log.Warning(exception, "数据验证失败: {Message}", validationEx.Message);
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                var validationResponse = new ApiResponse<IDictionary<string, string[]>>
                {
                    Code = 400,
                    Message = "数据验证失败",
                    Data = errors
                };
                return WriteJsonResponseAsync(context, validationResponse);

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Log.Error(exception, "服务器内部错误: {Message}", exception.Message);
                response = ApiResponse.Fail(
                    context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                        ? exception.ToString()
                        : "服务器内部错误，请稍后重试",
                    500);
                break;
        }

        return WriteJsonResponseAsync(context, response);
    }

    /// <summary>
    /// 写入JSON响应
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="response">响应对象</param>
    /// <returns>任务</returns>
    private static Task WriteJsonResponseAsync(HttpContext context, object response)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(response, options);
        return context.Response.WriteAsync(json);
    }
}
