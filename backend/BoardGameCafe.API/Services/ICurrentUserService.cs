namespace BoardGameCafe.API.Services;

/// <summary>
/// 当前用户服务接口
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// 获取当前用户Id
    /// </summary>
    /// <returns>用户Id，未登录返回null</returns>
    Guid? GetUserId();

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    /// <returns>用户名，未登录返回null</returns>
    string? GetUserName();

    /// <summary>
    /// 获取当前用户角色
    /// </summary>
    /// <returns>角色名称，未登录返回null</returns>
    string? GetRole();

    /// <summary>
    /// 检查当前用户是否已认证
    /// </summary>
    /// <returns>已认证返回true，否则返回false</returns>
    bool IsAuthenticated();

    /// <summary>
    /// 检查当前用户是否属于指定角色
    /// </summary>
    /// <param name="role">角色名称</param>
    /// <returns>属于指定角色返回true，否则返回false</returns>
    bool IsInRole(string role);
}
