using BoardGameCafe.Core.Interfaces;
using BoardGameCafe.Infrastructure.Data;
using BoardGameCafe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoardGameCafe.Infrastructure.Extensions;

/// <summary>
/// 依赖注入扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册基础设施层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="optionsAction">数据库上下文配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<BoardGameCafeDbContext>(optionsAction);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
