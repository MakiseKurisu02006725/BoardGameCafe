using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 损坏记录控制器
/// </summary>
[ApiController]
[Route("api/damages")]
[Produces("application/json")]
public class DamageController : ControllerBase
{
    private readonly IBoardGameService _boardGameService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="boardGameService">桌游服务</param>
    public DamageController(IBoardGameService boardGameService)
    {
        _boardGameService = boardGameService;
    }

    /// <summary>
    /// 损坏记录查询
    /// </summary>
    /// <returns>损坏记录列表</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<DamageRecordDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<DamageRecordDto>>>> GetDamages()
    {
        var result = await Task.FromResult(new List<DamageRecordDto>());
        return Ok(ApiResponse<List<DamageRecordDto>>.Success(result, "查询成功"));
    }
}
