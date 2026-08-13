namespace BoardGameCafe.Core.Enums;

/// <summary>
/// 支付方式
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// 微信支付
    /// </summary>
    WeChat = 1,

    /// <summary>
    /// 支付宝
    /// </summary>
    Alipay = 2,

    /// <summary>
    /// 现金
    /// </summary>
    Cash = 3,

    /// <summary>
    /// 会员余额
    /// </summary>
    MemberBalance = 4
}
