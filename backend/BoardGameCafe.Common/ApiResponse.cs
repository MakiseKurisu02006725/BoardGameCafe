namespace BoardGameCafe.Common;

/// <summary>
/// 通用API响应包装类（非泛型版本）
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 响应状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => Code >= 200 && Code < 300;

    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="message">响应消息</param>
    /// <returns>成功的API响应</returns>
    public static ApiResponse Success(string message = "操作成功")
    {
        return new ApiResponse
        {
            Code = 200,
            Message = message
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误状态码</param>
    /// <returns>失败的API响应</returns>
    public static ApiResponse Fail(string message = "操作失败", int code = 400)
    {
        return new ApiResponse
        {
            Code = code,
            Message = message
        };
    }
}

/// <summary>
/// 通用API响应包装类（泛型版本）
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 创建成功响应（带数据）
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="message">响应消息</param>
    /// <returns>成功的API响应</returns>
    public static ApiResponse<T> Success(T data, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建失败响应（带数据）
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误状态码</param>
    /// <param name="data">响应数据</param>
    /// <returns>失败的API响应</returns>
    public static new ApiResponse<T> Fail(string message = "操作失败", int code = 400, T? data = default)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message,
            Data = data
        };
    }
}

/// <summary>
/// 分页API响应包装类
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedApiResponse<T> : ApiResponse<PagedResult<T>>
{
    /// <summary>
    /// 创建分页成功响应
    /// </summary>
    /// <param name="pagedResult">分页结果</param>
    /// <param name="message">响应消息</param>
    /// <returns>分页成功的API响应</returns>
    public static PagedApiResponse<T> Success(PagedResult<T> pagedResult, string message = "操作成功")
    {
        return new PagedApiResponse<T>
        {
            Code = 200,
            Message = message,
            Data = pagedResult
        };
    }
}
