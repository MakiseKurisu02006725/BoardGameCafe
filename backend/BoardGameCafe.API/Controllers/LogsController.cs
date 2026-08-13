using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Infrastructure.Data;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 操作日志控制器
/// </summary>
[ApiController]
[Route("api/logs")]
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly IOperationLogService _operationLogService;
    private readonly BoardGameCafeDbContext _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="operationLogService">操作日志服务</param>
    /// <param name="db">数据库上下文（用于批量清理）</param>
    public LogsController(IOperationLogService operationLogService, BoardGameCafeDbContext db)
    {
        _operationLogService = operationLogService;
        _db = db;
    }

    /// <summary>
    /// 分页查询操作日志
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>操作日志分页列表</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(PagedApiResponse<OperationLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedApiResponse<OperationLogDto>>> GetLogs([FromQuery] LogQueryParams queryParams)
    {
        var result = await _operationLogService.GetPagedList(queryParams);
        return Ok(PagedApiResponse<OperationLogDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 手动清理历史日志：删除指定天数前的所有操作日志
    /// </summary>
    /// <param name="days">保留天数（默认 90），早于该天数的日志将被删除</param>
    /// <returns>删除条数</returns>
    [HttpPost("/api/operationlogs/cleanup")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<int>>> Cleanup(int days = 90)
    {
        if (days <= 0)
        {
            days = 90;
        }

        var cutoff = DateTime.Now.AddDays(-days);

        // ExecuteDeleteAsync 直接生成 DELETE SQL 批量删除，不将数据加载到内存
        var deleted = await _db.OperationLogs
            .Where(l => l.Timestamp < cutoff)
            .ExecuteDeleteAsync();

        // 记录本次清理日志（操作人为 System，类型为 CLEANUP）
        _db.OperationLogs.Add(new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = null,
            UserName = "System",
            Action = "CLEANUP",
            TargetId = null,
            Description = $"手动清理 {days} 天前历史日志，共删除 {deleted} 条",
            Timestamp = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<int>.Success(deleted, "清理成功"));
    }

    /// <summary>
    /// 获取日志清理配置（登录用户可访问）
    /// </summary>
    /// <returns>清理配置</returns>
    [HttpGet("/api/operationlogs/cleanup-settings")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CleanupSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CleanupSettingsDto>>> GetCleanupSettings()
    {
        var settings = await Task.FromResult(CleanupSettingsHelper.GetSettings(_db));
        return Ok(ApiResponse<CleanupSettingsDto>.Success(settings, "查询成功"));
    }

    /// <summary>
    /// 保存日志清理配置（仅管理员）
    /// </summary>
    /// <param name="dto">清理配置</param>
    /// <returns>操作结果</returns>
    [HttpPut("/api/operationlogs/cleanup-settings")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> UpdateCleanupSettings([FromBody] CleanupSettingsDto dto)
    {
        // 简单校验：天数 1-365，执行时间格式 HH:mm
        dto.QueryDays = Math.Clamp(dto.QueryDays, 1, 365);
        dto.OperationDays = Math.Clamp(dto.OperationDays, 1, 365);
        dto.LoginDays = Math.Clamp(dto.LoginDays, 1, 365);
        if (string.IsNullOrWhiteSpace(dto.ExecuteTime) ||
            !TimeSpan.TryParse(dto.ExecuteTime, out _))
        {
            dto.ExecuteTime = "02:00";
        }

        CleanupSettingsHelper.SaveSettings(_db, dto);
        await Task.CompletedTask;
        return Ok(ApiResponse.Success("保存成功"));
    }

    /// <summary>
    /// 清空全部操作日志（仅管理员），并记录一条清理日志
    /// </summary>
    /// <returns>删除条数</returns>
    [HttpDelete("/api/operationlogs/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<int>>> DeleteAllLogs()
    {
        // ExecuteDeleteAsync 直接生成 DELETE SQL 清空全表，不加载到内存
        var deleted = await _db.OperationLogs.ExecuteDeleteAsync();

        // 记录本次清空动作（操作人为当前管理员，类型为 CLEANUP）
        _db.OperationLogs.Add(new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = null,
            UserName = User.Identity?.Name ?? "System",
            Action = "CLEANUP",
            TargetId = null,
            Description = $"清空全部日志，共删除 {deleted} 条",
            Timestamp = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<int>.Success(deleted, "已清空全部日志"));
    }
}
