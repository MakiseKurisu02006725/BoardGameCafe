using BoardGameCafe.API.Services;

namespace BoardGameCafe.API.Extensions;

/// <summary>
/// API层服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加API层相关服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合，支持链式调用</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
