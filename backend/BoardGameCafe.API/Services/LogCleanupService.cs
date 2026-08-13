using BoardGameCafe.Application.Services;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameCafe.API.Services;

/// <summary>
/// 操作日志自动清理后台服务
/// 按数据库 SystemSettings 配置（保留天数/开关/执行时间）每天定时清理过期日志。
/// 使用 .NET 内置 BackgroundService + Timer 实现，无第三方调度库依赖。
/// </summary>
public class LogCleanupService : BackgroundService
{
    /// <summary>
    /// 默认每日清理时间（凌晨 2:00，配置缺失时使用）
    /// </summary>
    private static readonly TimeSpan DefaultRunTime = new(2, 0, 0);

    /// <summary>
    /// 自动清理被禁用时重新检查配置的间隔
    /// </summary>
    private static readonly TimeSpan DisabledCheckInterval = TimeSpan.FromMinutes(10);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogCleanupService> _logger;
    private Timer? _timer;

    /// <summary>
    /// 初始化日志清理服务
    /// </summary>
    /// <param name="serviceProvider">服务提供者（BackgroundService 为单例，需通过作用域获取 DbContext）</param>
    /// <param name="logger">日志记录器</param>
    public LogCleanupService(IServiceProvider serviceProvider, ILogger<LogCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 后台服务启动入口：注册定时器并按当前配置调度首次执行
    /// </summary>
    /// <param name="stoppingToken">停止令牌</param>
    /// <returns>任务</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(_ => _ = DoWorkAsync(), null, Timeout.Infinite, Timeout.Infinite);
        ScheduleNextRun();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行清理任务：先按数据库配置清理，再重新调度下次执行（异常不影响服务运行）
    /// </summary>
    private async Task DoWorkAsync()
    {
        try
        {
            await CleanupExpiredLogsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作日志自动清理失败");
        }
        finally
        {
            ScheduleNextRun();
        }
    }

    /// <summary>
    /// 根据数据库配置调度下次执行：
    /// 启用时计算到下次执行时间的延迟；禁用时低频轮询，配置重新启用后自动恢复
    /// </summary>
    private void ScheduleNextRun()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BoardGameCafeDbContext>();
            var cfg = CleanupSettingsHelper.GetSettings(db);

            if (!cfg.Enabled)
            {
                _timer?.Change(DisabledCheckInterval, Timeout.InfiniteTimeSpan);
                _logger.LogInformation("日志自动清理已禁用，{Interval} 后重新检查配置", DisabledCheckInterval);
                return;
            }

            if (!TimeSpan.TryParse(cfg.ExecuteTime, out var executeTime))
            {
                executeTime = DefaultRunTime;
            }

            var delay = GetDelayToNextRun(executeTime);
            _timer?.Change(delay, Timeout.InfiniteTimeSpan);
            _logger.LogInformation("日志自动清理已启用，下次执行时间 {NextTime}", DateTime.Now.Add(delay).ToString("yyyy-MM-dd HH:mm"));
        }
        catch (Exception ex)
        {
            // 读配置失败时按默认时间调度，保证服务不中断
            _logger.LogError(ex, "读取清理配置失败，按默认 {DefaultRunTime} 调度", DefaultRunTime);
            _timer?.Change(GetDelayToNextRun(DefaultRunTime), Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// 按数据库配置的保留天数差异化清理过期日志：
    /// QUERY / CREATE·UPDATE·DELETE / LOGIN·LOGOUT·CLEANUP 各按配置天数保留
    /// </summary>
    private async Task CleanupExpiredLogsAsync()
    {
        // BackgroundService 是单例，DbContext 是作用域服务，必须手动创建作用域获取
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BoardGameCafeDbContext>();
        var cfg = CleanupSettingsHelper.GetSettings(db);

        if (!cfg.Enabled)
        {
            return; // 已在调度层处理，此处双保险
        }

        var now = DateTime.Now;
        var queryCutoff = now.AddDays(-Math.Max(1, cfg.QueryDays));
        var changeCutoff = now.AddDays(-Math.Max(1, cfg.OperationDays));
        var authCutoff = now.AddDays(-Math.Max(1, cfg.LoginDays));

        // 分三类批量删除：ExecuteDeleteAsync 直接翻译为 DELETE SQL，不将数据加载到内存
        var queryCount = await db.OperationLogs
            .Where(l => l.Action == "QUERY" && l.Timestamp < queryCutoff)
            .ExecuteDeleteAsync();

        var changeCount = await db.OperationLogs
            .Where(l => (l.Action == "CREATE" || l.Action == "UPDATE" || l.Action == "DELETE") &&
                        l.Timestamp < changeCutoff)
            .ExecuteDeleteAsync();

        var authCount = await db.OperationLogs
            .Where(l => (l.Action == "LOGIN" || l.Action == "LOGOUT" || l.Action == "CLEANUP") &&
                        l.Timestamp < authCutoff)
            .ExecuteDeleteAsync();

        var total = queryCount + changeCount + authCount;
        if (total == 0)
        {
            _logger.LogInformation("操作日志自动清理完成，无需清理的日志");
            return;
        }

        // 记录本次清理结果（CLEANUP 类型日志，自身按登录类保留天数保留）
        db.OperationLogs.Add(new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = null,
            UserName = "System",
            Action = "CLEANUP",
            TargetId = null,
            Description = $"自动清理历史日志：删除查询日志 {queryCount} 条、变更日志 {changeCount} 条、登录日志 {authCount} 条",
            Timestamp = DateTime.Now
        });
        await db.SaveChangesAsync();

        _logger.LogInformation(
            "操作日志自动清理完成，共删除 {Total} 条（查询 {QueryCount}、变更 {ChangeCount}、登录 {AuthCount}）",
            total, queryCount, changeCount, authCount);
    }

    /// <summary>
    /// 计算到下一个指定时刻的等待时长
    /// </summary>
    /// <param name="runTime">每日执行时间</param>
    /// <returns>等待时长</returns>
    private static TimeSpan GetDelayToNextRun(TimeSpan runTime)
    {
        var now = DateTime.Now;
        var nextRun = now.Date.Add(runTime);
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1); // 今天的执行时间已过，顺延到明天
        }

        return nextRun - now;
    }

    /// <summary>
    /// 服务停止时释放定时器
    /// </summary>
    /// <param name="stoppingToken">停止令牌</param>
    /// <returns>任务</returns>
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        _logger.LogInformation("操作日志清理服务已停止");
        await base.StopAsync(stoppingToken);
    }
}
