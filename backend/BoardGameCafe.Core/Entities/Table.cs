using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 桌位实体
/// </summary>
public class Table
{
    /// <summary>
    /// 桌位主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 桌位编号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 桌位区域
    /// </summary>
    public TableArea Area { get; set; }

    /// <summary>
    /// 座位数
    /// </summary>
    public int Seats { get; set; }

    /// <summary>
    /// 桌位状态
    /// </summary>
    public TableStatus Status { get; set; }

    /// <summary>
    /// 每小时费率
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
