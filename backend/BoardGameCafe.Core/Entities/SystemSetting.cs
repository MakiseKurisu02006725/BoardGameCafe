namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 系统配置项实体（键值对存储，用于日志清理等运行时配置）
/// </summary>
public class SystemSetting
{
    /// <summary>
    /// 配置主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 配置键（唯一，如 LogCleanup:QueryDays）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 配置值（字符串存储，由读取方转换类型）
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 配置说明（可空）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
