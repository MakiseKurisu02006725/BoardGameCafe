namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 优惠券实体
/// </summary>
public class Coupon
{
    /// <summary>
    /// 优惠券主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 优惠券名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 折扣率（如0.8表示八折，与DiscountAmount二选一）
    /// </summary>
    public decimal? Discount { get; set; }

    /// <summary>
    /// 减免金额（与Discount二选一）
    /// </summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// 最低消费金额
    /// </summary>
    public decimal MinAmount { get; set; }

    /// <summary>
    /// 过期日期
    /// </summary>
    public DateTime ExpireDate { get; set; }

    /// <summary>
    /// 是否已使用
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// 使用该优惠券的订单Id
    /// </summary>
    public Guid? UsedOrderId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
