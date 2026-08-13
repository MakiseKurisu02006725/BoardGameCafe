using BoardGameCafe.Common;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 操作日志DTO
/// </summary>
public class OperationLogDto
{
    /// <summary>
    /// 操作日志主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 操作人用户Id
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作人用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 操作动作
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 操作目标Id
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// 操作描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 操作时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 操作日志查询参数DTO
/// </summary>
public class LogQueryParams : PaginationParams
{
    /// <summary>
    /// 关键词搜索（操作描述/目标Id）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 操作人用户Id筛选
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作动作筛选
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// 查询开始日期
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// 查询结束日期
    /// </summary>
    public DateTime? DateTo { get; set; }
}
