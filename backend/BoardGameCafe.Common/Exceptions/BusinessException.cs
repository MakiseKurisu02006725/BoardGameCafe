namespace BoardGameCafe.Common.Exceptions;

/// <summary>
/// 业务异常类
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// 初始化业务异常类的新实例
    /// </summary>
    public BusinessException()
        : base("发生业务错误")
    {
        ErrorCode = 400;
    }

    /// <summary>
    /// 使用指定的错误消息初始化业务异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    public BusinessException(string message)
        : base(message)
    {
        ErrorCode = 400;
    }

    /// <summary>
    /// 使用指定的错误消息和错误码初始化业务异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误码</param>
    public BusinessException(string message, int errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化业务异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = 400;
    }

    /// <summary>
    /// 使用指定的错误消息、错误码和内部异常初始化业务异常类的新实例
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误码</param>
    /// <param name="innerException">内部异常</param>
    public BusinessException(string message, int errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 抛出业务异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误码</param>
    /// <exception cref="BusinessException">始终抛出业务异常</exception>
    public static void Throw(string message, int errorCode = 400)
    {
        throw new BusinessException(message, errorCode);
    }

    /// <summary>
    /// 如果条件为真则抛出业务异常
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误码</param>
    /// <exception cref="BusinessException">当条件为真时抛出业务异常</exception>
    public static void ThrowIf(bool condition, string message, int errorCode = 400)
    {
        if (condition)
        {
            Throw(message, errorCode);
        }
    }

    /// <summary>
    /// 如果值为null则抛出业务异常
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="value">待检查的值</param>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误码</param>
    /// <returns>如果值不为null则返回原值</returns>
    /// <exception cref="BusinessException">当值为null时抛出业务异常</exception>
    public static T ThrowIfNull<T>(T? value, string message, int errorCode = 400)
        where T : class
    {
        if (value == null)
        {
            Throw(message, errorCode);
        }

        return value;
    }
}
