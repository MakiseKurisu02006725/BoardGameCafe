using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using System.Security.Claims;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 桌游管理控制器
/// </summary>
[ApiController]
[Route("api/boardgames")]
[Produces("application/json")]
public class BoardGamesController : ControllerBase
{
    private readonly IBoardGameService _boardGameService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="boardGameService">桌游服务</param>
    public BoardGamesController(IBoardGameService boardGameService)
    {
        _boardGameService = boardGameService;
    }

    /// <summary>
    /// 分页查询桌游列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>桌游分页列表</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedApiResponse<BoardGameDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<BoardGameDto>>> GetBoardGames([FromQuery] GameQueryParams queryParams)
    {
        var result = await _boardGameService.GetPagedList(queryParams);
        return Ok(PagedApiResponse<BoardGameDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取桌游详情
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <returns>桌游详情</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BoardGameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BoardGameDto>>> GetBoardGameById([FromRoute] Guid id)
    {
        var result = await _boardGameService.GetById(id);
        return Ok(ApiResponse<BoardGameDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 新增桌游
    /// </summary>
    /// <param name="dto">创建桌游请求参数</param>
    /// <returns>桌游详情</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<BoardGameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<BoardGameDto>>> CreateBoardGame([FromBody] CreateBoardGameDto dto)
    {
        var result = await _boardGameService.Create(dto);
        return Ok(ApiResponse<BoardGameDto>.Success(result, "创建成功"));
    }

    /// <summary>
    /// 编辑桌游
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="dto">更新桌游请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateBoardGame([FromRoute] Guid id, [FromBody] UpdateBoardGameDto dto)
    {
        await _boardGameService.Update(id, dto);
        return Ok(ApiResponse.Success("修改成功"));
    }

    /// <summary>
    /// 下架桌游（软删除）
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteBoardGame([FromRoute] Guid id)
    {
        await _boardGameService.Delete(id);
        return Ok(ApiResponse.Success("下架成功"));
    }

    /// <summary>
    /// 获取当前借出中的桌游列表（开台/预约借出，含关联订单号/预约号）
    /// </summary>
    /// <returns>借出中桌游列表</returns>
    [HttpGet("borrowed")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<List<BorrowedGameDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BorrowedGameDto>>>> GetBorrowedGames()
    {
        var result = await _boardGameService.GetBorrowed();
        return Ok(ApiResponse<List<BorrowedGameDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 借出桌游
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="quantity">借出数量</param>
    /// <returns>分配的副本编号列表</returns>
    [HttpPost("{id}/borrow")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<string>>>> BorrowGame([FromRoute] Guid id, [FromQuery] int quantity = 1)
    {
        var copyNos = await _boardGameService.Borrow(id, quantity);
        return Ok(ApiResponse<List<string>>.Success(copyNos, "借出成功"));
    }

    /// <summary>
    /// 补货桌游
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="dto">补货请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/restock")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> Restock([FromRoute] Guid id, [FromBody] RestockDto dto)
    {
        await _boardGameService.Restock(id, dto);
        return Ok(ApiResponse.Success("补货成功"));
    }

    /// <summary>
    /// 归还桌游
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="quantity">归还数量</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/return")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> ReturnGame([FromRoute] Guid id, [FromQuery] int quantity = 1)
    {
        await _boardGameService.Return(id, quantity);
        return Ok(ApiResponse.Success("归还成功"));
    }

    /// <summary>
    /// 登记损坏
    /// </summary>
    /// <param name="dto">损坏记录请求参数</param>
    /// <returns>损坏记录详情</returns>
    [HttpPost("damage")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<DamageRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DamageRecordDto>>> RecordDamage([FromBody] CreateDamageDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        var result = await _boardGameService.RecordDamage(dto, userId);
        return Ok(ApiResponse<DamageRecordDto>.Success(result, "登记成功"));
    }
}
