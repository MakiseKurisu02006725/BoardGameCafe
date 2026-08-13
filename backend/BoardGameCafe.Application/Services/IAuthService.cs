using BoardGameCafe.Application.DTOs;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto">登录请求DTO</param>
    /// <returns>登录响应DTO</returns>
    Task<LoginResponseDto> Login(LoginDto dto);

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="dto">注册请求DTO</param>
    /// <returns>登录响应DTO</returns>
    Task<LoginResponseDto> Register(RegisterDto dto);

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="dto">刷新令牌请求DTO</param>
    /// <returns>登录响应DTO</returns>
    Task<LoginResponseDto> RefreshToken(RefreshTokenDto dto);

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns>任务</returns>
    Task Logout(Guid userId);

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns>用户信息DTO</returns>
    Task<UserInfoDto> GetUserInfo(Guid userId);
}
