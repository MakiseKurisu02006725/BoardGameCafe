using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 统计分析控制器
/// </summary>
[ApiController]
[Route("api/statistics")]
[Produces("application/json")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="statisticsService">统计服务</param>
    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// 经营大屏数据
    /// </summary>
    /// <returns>仪表盘数据</returns>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard()
    {
        var result = await _statisticsService.GetDashboard();
        return Ok(ApiResponse<DashboardDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 营业额趋势
    /// </summary>
    /// <param name="days">统计天数，默认7天</param>
    /// <returns>趋势数据点列表</returns>
    [HttpGet("revenue-trend")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<TrendDataPointDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<TrendDataPointDto>>>> GetRevenueTrend([FromQuery] int days = 7)
    {
        var result = await _statisticsService.GetRevenueTrend(days);
        return Ok(ApiResponse<List<TrendDataPointDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 热门桌游排行
    /// </summary>
    /// <param name="topN">前N名，默认10</param>
    /// <returns>热门桌游列表</returns>
    [HttpGet("popular-games")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<PopularGameDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<PopularGameDto>>>> GetPopularGames([FromQuery] int topN = 10)
    {
        var result = await _statisticsService.GetPopularGames(topN);
        return Ok(ApiResponse<List<PopularGameDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// RFM分析
    /// </summary>
    /// <returns>会员RFM分析列表</returns>
    [HttpGet("member-rfm")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<MemberRfmDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<MemberRfmDto>>>> GetMemberRfm()
    {
        var result = await _statisticsService.GetMemberRfm();
        return Ok(ApiResponse<List<MemberRfmDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 时段客流统计（7天x24小时热力图数据）
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>客流统计列表</returns>
    [HttpGet("flow")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<FlowPointDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<FlowPointDto>>>> GetFlow([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _statisticsService.GetFlow(startDate, endDate);
        return Ok(ApiResponse<List<FlowPointDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 导出Excel报表
    /// </summary>
    /// <param name="reportType">报表类型</param>
    /// <param name="dateFrom">开始日期</param>
    /// <param name="dateTo">结束日期</param>
    /// <returns>Excel文件流</returns>
    [HttpGet("export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportExcel([FromQuery] string reportType, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var fileBytes = await _statisticsService.ExportExcel(reportType, dateFrom, dateTo);
        var fileName = $"{reportType}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
