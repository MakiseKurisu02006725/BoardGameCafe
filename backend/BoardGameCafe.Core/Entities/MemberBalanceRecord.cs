namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 会员余额流水记录实体
/// 记录会员余额变动（充值入账 / 消费扣款），用于核对余额构成
/// </summary>
public class MemberBalanceRecord
{
    /// <summary>
    /// 流水主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 流水类型：Recharge（充值）/ Consume（消费扣款）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 变动金额（消费扣款为负数，充值为正数）
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 关联订单Id（消费扣款时记录）
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 流水说明（如：消费扣款 订单号ORD...）
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
