using System.Reflection;
using BoardGameCafe.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BoardGameCafe.Application.Extensions;

/// <summary>
/// Application层依赖注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Application层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IBoardGameService, BoardGameService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOperationLogService, OperationLogService>();

        return services;
    }
}
