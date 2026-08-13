namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 登录请求DTO
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 注册请求DTO
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 注册密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮箱（可选）
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// 登录响应DTO
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// 访问令牌（JWT）
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 令牌过期时间（秒）
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户角色
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;
}

/// <summary>
/// 刷新令牌请求DTO
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 当前登录用户信息DTO（对应 /api/auth/me 返回）
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// 用户主键Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 会员Id（仅会员角色有值）
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 昵称（会员取NickName，员工取UserName）
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 角色枚举的字符串形式
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 头像URL（会员Avatar，员工默认空）
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Phone { get; set; }
}
