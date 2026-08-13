using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 操作日志服务接口
/// </summary>
public interface IOperationLogService
{
    /// <summary>
    /// 获取操作日志分页列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>操作日志分页结果</returns>
    Task<PagedResult<OperationLogDto>> GetPagedList(LogQueryParams queryParams);

    /// <summary>
    /// 记录操作日志
    /// </summary>
    /// <param name="userId">操作人用户Id</param>
    /// <param name="userName">操作人用户名</param>
    /// <param name="action">操作动作</param>
    /// <param name="targetId">操作目标Id</param>
    /// <param name="description">操作描述</param>
    /// <returns>任务</returns>
    Task LogAction(Guid? userId, string userName, string action, string? targetId, string? description);
}
