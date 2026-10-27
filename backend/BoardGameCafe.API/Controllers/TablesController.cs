using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using System.Security.Claims;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 桌位管理控制器
/// </summary>
[ApiController]
[Route("api/tables")]
[Produces("application/json")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="tableService">桌位服务</param>
    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    /// <summary>
    /// 获取所有桌位（实时状态）
    /// </summary>
    /// <returns>桌位列表</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<TableDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TableDto>>>> GetTables()
    {
        var result = await _tableService.GetAll();
        return Ok(ApiResponse<List<TableDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取桌位详情
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <returns>桌位详情</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TableDto>>> GetTableById([FromRoute] Guid id)
    {
        var result = await _tableService.GetById(id);
        return Ok(ApiResponse<TableDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 新增桌位
    /// </summary>
    /// <param name="dto">创建桌位请求参数</param>
    /// <returns>桌位详情</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<TableDto>>> CreateTable([FromBody] CreateTableDto dto)
    {
        var result = await _tableService.Create(dto);
        return Ok(ApiResponse<TableDto>.Success(result, "创建成功"));
    }

    /// <summary>
    /// 修改桌位
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <param name="dto">更新桌位请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateTable([FromRoute] Guid id, [FromBody] UpdateTableDto dto)
    {
        await _tableService.Update(id, dto);
        return Ok(ApiResponse.Success("修改成功"));
    }

    /// <summary>
    /// 设置/解除桌位维修状态
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <param name="dto">维修状态请求参数（status: maintain/normal）</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}/maintain")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> MaintainTable([FromRoute] Guid id, [FromBody] MaintainTableDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var operatorId = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var uid) ? uid : (Guid?)null;
        await _tableService.Maintain(id, dto.Status, operatorId);
        return Ok(ApiResponse.Success("操作成功"));
    }

    /// <summary>
    /// 删除桌位
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteTable([FromRoute] Guid id)
    {
        await _tableService.Delete(id);
        return Ok(ApiResponse.Success("删除成功"));
    }

    /// <summary>
    /// 开台
    /// </summary>
    /// <param name="dto">开台请求参数</param>
    /// <returns>订单详情</returns>
    [HttpPost("open")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> OpenTable([FromBody] OpenTableDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        var result = await _tableService.OpenTable(dto, userId);
        return Ok(ApiResponse<OrderDto>.Success(result, "开台成功"));
    }

    /// <summary>
    /// 换桌
    /// </summary>
    /// <param name="dto">换桌请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("change")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> ChangeTable([FromBody] ChangeTableDto dto)
    {
        await _tableService.ChangeTable(dto);
        return Ok(ApiResponse.Success("换桌成功"));
    }

    /// <summary>
    /// 结账关台
    /// </summary>
    /// <param name="dto">关台请求参数</param>
    /// <returns>订单详情</returns>
    [HttpPost("close")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> CloseTable([FromBody] CloseTableDto dto)
    {
        var result = await _tableService.CloseTable(dto);
        return Ok(ApiResponse<OrderDetailDto>.Success(result, "结账成功"));
    }
}
