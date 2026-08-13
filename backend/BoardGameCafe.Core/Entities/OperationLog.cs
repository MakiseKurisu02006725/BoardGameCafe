namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 操作日志实体
/// </summary>
public class OperationLog
{
    /// <summary>
    /// 操作日志主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 操作人用户Id（可为空，表示系统操作）
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
