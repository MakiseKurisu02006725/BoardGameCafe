using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 订单管理控制器
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="orderService">订单服务</param>
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// 分页查询订单
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>订单分页列表</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedApiResponse<OrderDto>>> GetOrders([FromQuery] OrderQueryParams queryParams)
    {
        var result = await _orderService.GetPagedList(queryParams);
        return Ok(PagedApiResponse<OrderDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取订单详情（含订单项）
    /// </summary>
    /// <param name="id">订单Id</param>
    /// <returns>订单详情</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrderById([FromRoute] Guid id)
    {
        var result = await _orderService.GetDetail(id);
        return Ok(ApiResponse<OrderDetailDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 支付订单
    /// </summary>
    /// <param name="dto">支付请求参数</param>
    /// <returns>订单详情</returns>
    [HttpPost("pay")]
    [Authorize(Roles = "Admin,Manager,Cashier,Member")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> Pay([FromBody] PaymentDto dto)
    {
        var result = await _orderService.Pay(dto);
        return Ok(ApiResponse<OrderDetailDto>.Success(result, "支付成功"));
    }

    /// <summary>
    /// 订单退款
    /// </summary>
    /// <param name="id">订单Id</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Refund([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var operatorId = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var uid) ? uid : (Guid?)null;
        await _orderService.Refund(id, operatorId);
        return Ok(ApiResponse.Success("退款成功"));
    }
}
