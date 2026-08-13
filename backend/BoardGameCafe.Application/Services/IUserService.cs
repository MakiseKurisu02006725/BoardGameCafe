using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 用户员工服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>用户分页结果</returns>
    Task<PagedResult<UserDto>> GetPagedList(UserQueryParams queryParams);

    /// <summary>
    /// 根据Id获取用户信息
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <returns>用户信息DTO</returns>
    Task<UserDto> GetById(Guid id);

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="dto">创建用户请求DTO</param>
    /// <returns>用户信息DTO</returns>
    Task<UserDto> Create(CreateUserDto dto);

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <param name="dto">更新用户请求DTO</param>
    /// <returns>任务</returns>
    Task Update(Guid id, UpdateUserDto dto);

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <returns>任务</returns>
    Task Delete(Guid id);

    /// <summary>
    /// 修改密码（用户修改自己密码，需要原密码验证）
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <param name="dto">修改密码请求DTO</param>
    /// <returns>任务</returns>
    Task ChangePassword(Guid id, ChangePasswordDto dto);

    /// <summary>
    /// 管理员重置用户密码（无需原密码）
    /// </summary>
    /// <param name="id">用户Id</param>
    /// <param name="dto">重置密码请求DTO</param>
    /// <returns>任务</returns>
    Task ResetPassword(Guid id, ResetPasswordDto dto);
}
