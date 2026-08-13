using BoardGameCafe.Common;
using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 员工用户信息DTO
/// </summary>
public class UserDto
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
    /// 用户角色枚举值
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// 用户角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

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
    /// 是否已设置密码
    /// </summary>
    public bool HasPassword { get; set; } = true;
}

/// <summary>
/// 创建用户请求DTO
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 初始密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// 电子邮箱（可选）
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// 更新用户请求DTO
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// 手机号码（可选）
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 电子邮箱（可选）
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 用户角色（可选）
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// 新密码（可选，留空不修改）
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// 修改密码请求DTO（用户修改自己密码，需要原密码）
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// 旧密码
    /// </summary>
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 管理员重置用户密码DTO（无需原密码）
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 用户查询参数
/// </summary>
public class UserQueryParams : PaginationParams
{
    /// <summary>
    /// 关键词（用户名/手机号）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 角色筛选
    /// </summary>
    public UserRole? Role { get; set; }
}
