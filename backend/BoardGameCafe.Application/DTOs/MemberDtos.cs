using BoardGameCafe.Common;
using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 会员信息DTO
/// </summary>
public class MemberDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// 会员编号（如 M128）
    /// </summary>
    public string MemberNo { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public string NickName { get; set; } = string.Empty;
    public MemberLevel Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int Points { get; set; }
    public decimal Balance { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 更新会员资料请求DTO
/// </summary>
public class UpdateProfileDto
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 头像地址
    /// </summary>
    public string? Avatar { get; set; }
}

/// <summary>
/// 会员充值请求DTO
/// </summary>
public class RechargeDto
{
    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 充值金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 赠送金额（可选；不传时按充值阶梯规则自动计算）
    /// </summary>
    public decimal? Bonus { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentMethod PayMethod { get; set; }
}

/// <summary>
/// 充值记录DTO
/// </summary>
public class RechargeRecordDto
{
    /// <summary>
    /// 充值记录Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 会员昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

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
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建会员请求DTO
/// </summary>
public class CreateMemberDto
{
    public string NickName { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public MemberLevel? Level { get; set; }
    public string? Password { get; set; }
}

public class UpdateMemberDto
{
    public string NickName { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public MemberLevel? Level { get; set; }
}

/// <summary>
/// 会员查询参数DTO
/// </summary>
public class MemberQueryParams : PaginationParams
{
    /// <summary>
    /// 关键词搜索（昵称/手机号）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 会员等级筛选
    /// </summary>
    public MemberLevel? Level { get; set; }
}

/// <summary>
/// 会员优惠券DTO
/// </summary>
public class MemberCouponDto
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public string CouponName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

/// <summary>
/// 会员余额流水DTO
/// </summary>
public class MemberBalanceRecordDto
{
    /// <summary>
    /// 流水Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 流水类型名称（充值/消费扣款）
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 变动金额（消费为负，充值为正）
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 关联订单号（消费扣款时）
    /// </summary>
    public string? OrderNo { get; set; }

    /// <summary>
    /// 流水说明
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
