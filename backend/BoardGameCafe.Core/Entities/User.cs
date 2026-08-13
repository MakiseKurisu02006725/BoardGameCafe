using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 用户实体
/// </summary>
public class User
{
    /// <summary>
    /// 用户主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希值
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }
}
