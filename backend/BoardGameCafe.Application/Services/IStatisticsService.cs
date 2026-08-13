using BoardGameCafe.Application.DTOs;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 统计服务接口
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// 获取仪表盘数据
    /// </summary>
    /// <returns>仪表盘DTO</returns>
    Task<DashboardDto> GetDashboard();

    /// <summary>
    /// 获取营收趋势数据
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <returns>趋势数据点列表</returns>
    Task<List<TrendDataPointDto>> GetRevenueTrend(int days = 7);

    /// <summary>
    /// 获取热门桌游排行
    /// </summary>
    /// <param name="topN">前N名</param>
    /// <returns>热门桌游列表</returns>
    Task<List<PopularGameDto>> GetPopularGames(int topN = 10);

    /// <summary>
    /// 获取会员RFM分析
    /// </summary>
    /// <returns>会员RFM列表</returns>
    Task<List<MemberRfmDto>> GetMemberRfm();

    /// <summary>
    /// 获取时段客流统计（按开台日期+小时分组统计订单数）
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>客流统计列表</returns>
    Task<List<FlowPointDto>> GetFlow(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 导出Excel报表
    /// </summary>
    /// <param name="reportType">报表类型</param>
    /// <param name="dateFrom">开始日期</param>
    /// <param name="dateTo">结束日期</param>
    /// <returns>Excel文件字节数组</returns>
    Task<byte[]> ExportExcel(string reportType, DateTime? dateFrom, DateTime? dateTo);
}
