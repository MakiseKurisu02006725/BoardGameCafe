using System.Security.Claims;

namespace BoardGameCafe.API.Services;

/// <summary>
/// 当前用户服务实现
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    /// <summary>
    /// HTTP上下文访问器
    /// </summary>
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 初始化当前用户服务的新实例
    /// </summary>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取当前用户Id
    /// </summary>
    /// <returns>用户Id，未登录返回null</returns>
    public Guid? GetUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
        {
            return null;
        }

        if (Guid.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    /// <returns>用户名，未登录返回null</returns>
    public string? GetUserName()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name);
        return claim?.Value;
    }

    /// <summary>
    /// 获取当前用户角色
    /// </summary>
    /// <returns>角色名称，未登录返回null</returns>
    public string? GetRole()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role);
        return claim?.Value;
    }

    /// <summary>
    /// 检查当前用户是否已认证
    /// </summary>
    /// <returns>已认证返回true，否则返回false</returns>
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// 检查当前用户是否属于指定角色
    /// </summary>
    /// <param name="role">角色名称</param>
    /// <returns>属于指定角色返回true，否则返回false</returns>
    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
    }
}
