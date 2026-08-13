using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 预约实体
/// </summary>
public class Reservation
{
    /// <summary>
    /// 预约主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 预约编号（如 RESV001）
    /// </summary>
    public string ReservationNo { get; set; } = string.Empty;

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 桌位Id（可为空，仅预约区域类型）
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// 预约桌游名称（逗号分隔，用于展示）
    /// </summary>
    public string? ReservedGames { get; set; }

    /// <summary>
    /// 预约桌游Id列表（逗号分隔的Guid，用于确认时自动借出副本）
    /// </summary>
    public string? ReservedGameIds { get; set; }

    /// <summary>
    /// 桌位区域类型
    /// </summary>
    public TableArea TableType { get; set; }

    /// <summary>
    /// 预约日期
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// 客人数量
    /// </summary>
    public int GuestCount { get; set; }

    /// <summary>
    /// 押金
    /// </summary>
    public decimal Deposit { get; set; }

    /// <summary>
    /// 预约状态
    /// </summary>
    public ReservationStatus Status { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
