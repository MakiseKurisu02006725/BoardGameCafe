using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 充值记录实体
/// </summary>
public class RechargeRecord
{
    /// <summary>
    /// 充值记录主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 充值金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 赠送金额
    /// </summary>
    public decimal Bonus { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentMethod PayMethod { get; set; }

    /// <summary>
    /// 创建人Id（可为空，表示会员自助充值）
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
