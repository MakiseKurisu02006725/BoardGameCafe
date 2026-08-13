using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 会员实体
/// </summary>
public class Member
{
    /// <summary>
    /// 会员主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 会员编号（如 M128）
    /// </summary>
    public string MemberNo { get; set; } = string.Empty;

    /// <summary>
    /// 关联用户Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 会员等级
    /// </summary>
    public MemberLevel Level { get; set; }

    /// <summary>
    /// 积分
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
