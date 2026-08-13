namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 日志清理配置DTO
/// </summary>
public class CleanupSettingsDto
{
    /// <summary>
    /// 查询类日志（QUERY）保留天数
    /// </summary>
    public int QueryDays { get; set; } = 30;

    /// <summary>
    /// 数据变更类日志（CREATE/UPDATE/DELETE）保留天数
    /// </summary>
    public int OperationDays { get; set; } = 90;

    /// <summary>
    /// 登录登出类日志（LOGIN/LOGOUT）保留天数
    /// </summary>
    public int LoginDays { get; set; } = 180;

    /// <summary>
    /// 是否启用每日自动清理
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 每日执行清理时间（HH:mm）
    /// </summary>
    public string ExecuteTime { get; set; } = "02:00";
}
