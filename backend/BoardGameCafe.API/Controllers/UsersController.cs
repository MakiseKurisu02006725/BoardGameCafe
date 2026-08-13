using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using System.Security.Claims;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 员工用户管理控制器
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userService">用户服务</param>
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 分页查询用户
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>用户分页列表</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(PagedApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedApiResponse<UserDto>>> GetUsers([FromQuery] UserQueryParams queryParams)
    {
        var result = await _userService.GetPagedList(queryParams);
        return Ok(PagedApiResponse<UserDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <returns>用户详情</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById([FromRoute] Guid id)
    {
        var result = await _userService.GetById(id);
        return Ok(ApiResponse<UserDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="dto">创建用户请求参数</param>
    /// <returns>用户详情</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.Create(dto);
        return Ok(ApiResponse<UserDto>.Success(result, "创建成功"));
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <param name="dto">更新用户请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserDto dto)
    {
        await _userService.Update(id, dto);
        return Ok(ApiResponse.Success("修改成功"));
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteUser([FromRoute] Guid id)
    {
        await _userService.Delete(id);
        return Ok(ApiResponse.Success("删除成功"));
    }

    /// <summary>
    /// 修改自己密码
    /// </summary>
    /// <param name="dto">修改密码请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        await _userService.ChangePassword(userId, dto);
        return Ok(ApiResponse.Success("修改成功"));
    }

    /// <summary>
    /// 管理员重置用户密码（无需原密码）
    /// </summary>
    /// <param name="id">目标用户Id</param>
    /// <param name="dto">重置密码请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromRoute] Guid id, [FromBody] ResetPasswordDto dto)
    {
        await _userService.ResetPassword(id, dto);
        return Ok(ApiResponse.Success("密码重置成功"));
    }
}
