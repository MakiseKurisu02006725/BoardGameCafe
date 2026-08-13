namespace BoardGameCafe.Core.Enums;

/// <summary>
/// 预约状态
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// 待确认
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 已确认
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 5
}
