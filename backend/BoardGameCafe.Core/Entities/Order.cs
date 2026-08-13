using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 订单实体
/// </summary>
public class Order
{
    /// <summary>
    /// 订单主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 订单编号（如 ORD202408100003）
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 会员Id（可为空，表示非会员消费）
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// 关联预约Id（预约转开台时记录，结账后自动归还预约桌游）
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// 桌位Id（可为空，表示无桌位消费）
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 折扣折数（0.1 ~ 10，10 折 = 原价无优惠）
    /// </summary>
    public decimal DiscountRate { get; set; } = 10m;

    /// <summary>
    /// 折扣金额
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// 实付金额
    /// </summary>
    public decimal ActualAmount { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// 创建人Id
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
