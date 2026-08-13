namespace BoardGameCafe.Common.Exceptions;

/// <summary>
/// 未授权异常类
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// 初始化未授权异常类的新实例
    /// </summary>
    public UnauthorizedException()
        : base("未授权访问，请先登录")
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化未授权异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化未授权异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 抛出未授权异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <exception cref="UnauthorizedException">始终抛出未授权异常</exception>
    public static void Throw(string message = "未授权访问，请先登录")
    {
        throw new UnauthorizedException(message);
    }

    /// <summary>
    /// 如果条件为真则抛出未授权异常
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="message">错误消息</param>
    /// <exception cref="UnauthorizedException">当条件为真时抛出未授权异常</exception>
    public static void ThrowIf(bool condition, string message = "未授权访问，请先登录")
    {
        if (condition)
        {
            Throw(message);
        }
    }

    /// <summary>
    /// 抛出权限不足异常
    /// </summary>
    /// <param name="permission">所需权限名称</param>
    /// <exception cref="UnauthorizedException">始终抛出未授权异常</exception>
    public static void ThrowForbidden(string? permission = null)
    {
        var message = string.IsNullOrWhiteSpace(permission)
            ? "权限不足，无法执行该操作"
            : $"权限不足，需要权限：{permission}";

        throw new UnauthorizedException(message);
    }

    /// <summary>
    /// 如果条件为真则抛出权限不足异常
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="permission">所需权限名称</param>
    /// <exception cref="UnauthorizedException">当条件为真时抛出未授权异常</exception>
    public static void ThrowForbiddenIf(bool condition, string? permission = null)
    {
        if (condition)
        {
            ThrowForbidden(permission);
        }
    }

    /// <summary>
    /// 抛出Token过期异常
    /// </summary>
    /// <exception cref="UnauthorizedException">始终抛出未授权异常</exception>
    public static void ThrowTokenExpired()
    {
        throw new UnauthorizedException("登录状态已过期，请重新登录");
    }

    /// <summary>
    /// 抛出Token无效异常
    /// </summary>
    /// <exception cref="UnauthorizedException">始终抛出未授权异常</exception>
    public static void ThrowTokenInvalid()
    {
        throw new UnauthorizedException("登录凭证无效，请重新登录");
    }
}
