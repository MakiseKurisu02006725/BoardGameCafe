using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using System.Security.Claims;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="authService">认证服务</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto">登录请求参数</param>
    /// <returns>登录响应（包含令牌）</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.Login(dto);
        return Ok(ApiResponse<LoginResponseDto>.Success(result, "登录成功"));
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="dto">注册请求参数</param>
    /// <returns>注册响应（包含令牌）</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.Register(dto);
        return Ok(ApiResponse<LoginResponseDto>.Success(result, "注册成功"));
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="dto">刷新令牌请求参数</param>
    /// <returns>新的令牌响应</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshToken(dto);
        return Ok(ApiResponse<LoginResponseDto>.Success(result, "刷新成功"));
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <returns>登出结果</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        await _authService.Logout(userId);
        return Ok(ApiResponse.Success("登出成功"));
    }

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetCurrentUserInfo()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        var info = await _authService.GetUserInfo(userId);
        return Ok(ApiResponse<UserInfoDto>.Success(info, "获取成功"));
    }
}
