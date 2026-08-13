using AutoMapper;
using BoardGameCafe.API.Services;
using BoardGameCafe.Common;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// API控制器基类，提供通用功能和快捷方法
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// 当前用户服务
    /// </summary>
    protected readonly ICurrentUserService CurrentUserService;

    /// <summary>
    /// 对象映射器
    /// </summary>
    protected readonly IMapper Mapper;

    /// <summary>
    /// 初始化API控制器基类的新实例
    /// </summary>
    /// <param name="currentUserService">当前用户服务</param>
    /// <param name="mapper">对象映射器</param>
    protected ApiControllerBase(
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        CurrentUserService = currentUserService;
        Mapper = mapper;
    }

    /// <summary>
    /// 返回成功响应（无数据）
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns>200 OK 成功响应</returns>
    protected ActionResult<ApiResponse> SuccessResult(string message = "操作成功")
    {
        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 返回成功响应（带数据）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">响应数据</param>
    /// <param name="message">成功消息</param>
    /// <returns>200 OK 成功响应（带数据）</returns>
    protected ActionResult<ApiResponse<T>> SuccessResult<T>(T data, string message = "操作成功")
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    /// <summary>
    /// 返回失败响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误状态码（默认400）</param>
    /// <returns>失败响应</returns>
    protected ActionResult<ApiResponse> FailResult(string message = "操作失败", int code = 400)
    {
        return code switch
        {
            404 => NotFound(ApiResponse.Fail(message, code)),
            401 => Unauthorized(ApiResponse.Fail(message, code)),
            403 => Forbid(),
            _ => BadRequest(ApiResponse.Fail(message, code))
        };
    }

    /// <summary>
    /// 返回失败响应（带数据）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误状态码（默认400）</param>
    /// <param name="data">响应数据</param>
    /// <returns>失败响应（带数据）</returns>
    protected ActionResult<ApiResponse<T>> FailResult<T>(string message = "操作失败", int code = 400, T? data = default)
    {
        var response = ApiResponse<T>.Fail(message, code, data);

        return code switch
        {
            404 => NotFound(response),
            401 => Unauthorized(response),
            _ => BadRequest(response)
        };
    }

    /// <summary>
    /// 获取当前用户Id
    /// </summary>
    /// <returns>用户Id，未登录返回null</returns>
    protected Guid? CurrentUserId => CurrentUserService.GetUserId();

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    /// <returns>用户名，未登录返回null</returns>
    protected string? CurrentUserName => CurrentUserService.GetUserName();

    /// <summary>
    /// 检查当前用户是否已认证
    /// </summary>
    /// <returns>已认证返回true，否则返回false</returns>
    protected bool IsAuthenticated => CurrentUserService.IsAuthenticated();
}
