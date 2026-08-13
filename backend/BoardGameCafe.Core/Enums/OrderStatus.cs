namespace BoardGameCafe.Core.Enums;

/// <summary>
/// 订单状态
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// 待支付
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 已支付
    /// </summary>
    Paid = 2,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// 已退款
    /// </summary>
    Refunded = 4
}
