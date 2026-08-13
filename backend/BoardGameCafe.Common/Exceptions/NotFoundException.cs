namespace BoardGameCafe.Common.Exceptions;

/// <summary>
/// 未找到异常类
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// 实体名称
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// 实体标识
    /// </summary>
    public object? EntityKey { get; set; }

    /// <summary>
    /// 初始化未找到异常类的新实例
    /// </summary>
    public NotFoundException()
        : base("请求的资源未找到")
    {
        EntityName = string.Empty;
    }

    /// <summary>
    /// 使用指定的错误消息初始化未找到异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    public NotFoundException(string message)
        : base(message)
    {
        EntityName = string.Empty;
    }

    /// <summary>
    /// 使用指定的实体名称初始化未找到异常类的新实例
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityKey">实体标识</param>
    public NotFoundException(string entityName, object entityKey)
        : base($"未找到 {entityName}，标识：{entityKey}")
    {
        EntityName = entityName;
        EntityKey = entityKey;
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化未找到异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        EntityName = string.Empty;
    }

    /// <summary>
    /// 抛出未找到异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <exception cref="NotFoundException">始终抛出未找到异常</exception>
    public static void Throw(string message = "请求的资源未找到")
    {
        throw new NotFoundException(message);
    }

    /// <summary>
    /// 使用实体名称和标识抛出未找到异常
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityKey">实体标识</param>
    /// <exception cref="NotFoundException">始终抛出未找到异常</exception>
    public static void Throw(string entityName, object entityKey)
    {
        throw new NotFoundException(entityName, entityKey);
    }

    /// <summary>
    /// 如果条件为真则抛出未找到异常
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="message">错误消息</param>
    /// <exception cref="NotFoundException">当条件为真时抛出未找到异常</exception>
    public static void ThrowIf(bool condition, string message = "请求的资源未找到")
    {
        if (condition)
        {
            Throw(message);
        }
    }

    /// <summary>
    /// 如果值为null则抛出未找到异常
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="value">待检查的值</param>
    /// <param name="message">错误消息</param>
    /// <returns>如果值不为null则返回原值</returns>
    /// <exception cref="NotFoundException">当值为null时抛出未找到异常</exception>
    public static T ThrowIfNull<T>(T? value, string message = "请求的资源未找到")
        where T : class
    {
        if (value == null)
        {
            Throw(message);
        }

        return value;
    }

    /// <summary>
    /// 如果值为null则抛出未找到异常（带实体信息）
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="value">待检查的值</param>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityKey">实体标识</param>
    /// <returns>如果值不为null则返回原值</returns>
    /// <exception cref="NotFoundException">当值为null时抛出未找到异常</exception>
    public static T ThrowIfNull<T>(T? value, string entityName, object entityKey)
        where T : class
    {
        if (value == null)
        {
            Throw(entityName, entityKey);
        }

        return value;
    }
}
