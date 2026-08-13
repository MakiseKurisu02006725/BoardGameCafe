using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using BoardGameCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 统计服务实现
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BoardGameCafeDbContext _db;
    private readonly ILogger<StatisticsService> _logger;

    /// <summary>
    /// 初始化统计服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="db">数据库上下文（统计只读查询使用，AsNoTracking 不跟踪实体）</param>
    /// <param name="logger">日志记录器</param>
    public StatisticsService(
        IUnitOfWork unitOfWork,
        BoardGameCafeDbContext db,
        ILogger<StatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DashboardDto> GetDashboard()
    {
        // TODO: 实现仪表盘统计
        // 1. 今日营收（已支付订单的ActualAmount之和）
        // 2. 今日订单数
        // 3. 桌位上客率（占用中桌位数/总桌位数）
        // 4. 当前在店人数（占用中桌位数的座位数之和）
        // 5. 近7天营收趋势
        // 6. 热门桌游Top5

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todayOrders = _unitOfWork.Orders
            .Find(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status == OrderStatus.Paid)
            .ToList();

        var todayRevenue = todayOrders.Sum(o => o.ActualAmount);
        var todayOrderCount = todayOrders.Count;

        // 昨日数据（用于计算环比）
        var yesterdayOrders = _unitOfWork.Orders
            .Find(o => o.CreatedAt >= today.AddDays(-1) && o.CreatedAt < today && o.Status == OrderStatus.Paid)
            .ToList();
        var yesterdayRevenue = yesterdayOrders.Sum(o => o.ActualAmount);
        var yesterdayOrderCount = yesterdayOrders.Count;

        var allTables = _unitOfWork.Tables.GetAll().ToList();
        var totalTables = allTables.Count;
        var occupiedTables = allTables.Where(t => t.Status == TableStatus.Occupied).ToList();
        var occupiedCount = occupiedTables.Count;
        var openRate = totalTables > 0 ? Math.Round((decimal)occupiedCount / totalTables * 100, 2) : 0;
        var inStorePeople = occupiedTables.Sum(t => t.Seats);

        var result = new DashboardDto
        {
            TodayRevenue = todayRevenue,
            TodayOrders = todayOrderCount,
            OpenRate = openRate,
            InStorePeople = inStorePeople,
            RevenueTrendPercent = CalcPercentChange(todayRevenue, yesterdayRevenue),
            OrdersTrendPercent = CalcPercentChange(todayOrderCount, yesterdayOrderCount),
            RevenueTrend = GetRevenueTrendInternal(7),
            PopularGames = await GetPopularGames(5)
        };

        return result;
    }

    /// <summary>
    /// 计算环比百分比：(本期-上期)/上期*100。
    /// 上期为0时：本期也为0返回0（0%），本期有数据返回null（无法计算环比）。
    /// </summary>
    /// <param name="current">本期数值</param>
    /// <param name="previous">上期数值</param>
    /// <returns>环比百分比，null表示无环比数据</returns>
    private static decimal? CalcPercentChange(decimal current, decimal previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : null;
        }

        return Math.Round((current - previous) / previous * 100, 2);
    }

    /// <inheritdoc />
    public Task<List<TrendDataPointDto>> GetRevenueTrend(int days = 7)
    {
        var result = GetRevenueTrendInternal(days);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<List<PopularGameDto>> GetPopularGames(int topN = 10)
    {
        // 只读统计查询：AsNoTracking 不跟踪实体；分组聚合由数据库端执行，避免全表加载到内存
        return await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Type == OrderItemType.GameLease)
            .GroupBy(i => i.Name)
            .Select(g => new PopularGameDto
            {
                Name = g.Key,
                Count = g.Sum(i => i.Quantity)
            })
            .OrderByDescending(g => g.Count)
            .Take(topN)
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<MemberRfmDto>> GetMemberRfm()
    {
        // RFM 分析：
        // R(Recency) 最近一次消费（MAX(EndTime)）距今天数
        // F(Frequency) 消费频次（已支付订单数）
        // M(Monetary) 消费总额（SUM(ActualAmount)）
        // 三个维度均按会员整体排名分位打 1~5 分，分群以全体会员的中位数为高低阈值

        var members = _unitOfWork.Members.GetAll().ToList();
        var now = DateTime.Now;

        // 1. 计算每个会员的 R/F/M 原始值
        var stats = new List<(Member Member, int Recency, int Frequency, decimal Monetary)>();
        foreach (var member in members)
        {
            var orders = _unitOfWork.Orders
                .Find(o => o.MemberId == member.Id && o.Status == OrderStatus.Paid)
                .ToList();

            // 最近消费时间：取已支付订单的 MAX(EndTime)，未结账（EndTime为空）时退回 CreatedAt
            var lastConsume = orders
                .Select(o => o.EndTime ?? o.CreatedAt)
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            stats.Add((
                member,
                orders.Count > 0 ? (int)(now - lastConsume).TotalDays : 999,
                orders.Count,
                orders.Sum(o => o.ActualAmount)
            ));
        }

        // 2. 分位打分（1~5）：R 越小越好（升序），F/M 越大越好（降序）
        var rScores = ScoreByPercentile(stats.Select(s => (double)s.Recency).ToList(), ascending: true);
        var fScores = ScoreByPercentile(stats.Select(s => (double)s.Frequency).ToList(), ascending: false);
        var mScores = ScoreByPercentile(stats.Select(s => (double)s.Monetary).ToList(), ascending: false);

        // 3. 中位数阈值：R 高=天数小于等于中位数，F/M 高=大于中位数
        var rMedian = Median(stats.Select(s => (double)s.Recency));
        var fMedian = Median(stats.Select(s => (double)s.Frequency));
        var mMedian = Median(stats.Select(s => (double)s.Monetary));

        var result = new List<MemberRfmDto>();
        for (var i = 0; i < stats.Count; i++)
        {
            var (member, recency, frequency, monetary) = stats[i];
            var rScore = rScores[i];
            var fScore = fScores[i];
            var mScore = mScores[i];

            // 新客户单独处理：仅消费 1 次 或 注册不足 30 天，不纳入 8 分群
            var isNew = frequency == 1 || member.CreatedAt > now.AddDays(-30);
            var tag = isNew
                ? "新客户"
                : GetRfmGroup(recency, frequency, monetary, rMedian, fMedian, mMedian);

            result.Add(new MemberRfmDto
            {
                MemberId = member.Id,
                NickName = member.NickName,
                Recency = recency,
                Frequency = frequency,
                Monetary = monetary,
                RfmScore = (int)Math.Round((rScore + fScore + mScore) / 15m * 100),
                Tag = tag
            });
        }

        return Task.FromResult(result.OrderByDescending(r => r.RfmScore).ToList());
    }

    /// <inheritdoc />
    public Task<List<FlowPointDto>> GetFlow(DateTime startDate, DateTime endDate)
    {
        // 按开台时间（StartTime）的日期+小时分组统计订单数
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1); // 包含结束日期当天

        var result = _unitOfWork.Orders
            .Find(o => o.StartTime >= start && o.StartTime < end)
            .GroupBy(o => new { o.StartTime.Date, o.StartTime.Hour })
            .Select(g => new FlowPointDto
            {
                Date = g.Key.Date,
                Hour = g.Key.Hour,
                Count = g.Count()
            })
            .OrderBy(f => f.Date)
            .ThenBy(f => f.Hour)
            .ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// 按排名分位为每个值打分（1~5 分）
    /// </summary>
    /// <param name="values">原始值列表</param>
    /// <param name="ascending">true 表示值越小越好（如R天数），false 表示值越大越好（如F/M）</param>
    /// <returns>与输入顺序一致的分值列表</returns>
    private static List<int> ScoreByPercentile(List<double> values, bool ascending)
    {
        var n = values.Count;
        if (n == 0)
        {
            return new List<int>();
        }

        // 按好坏排序（rank=0 为表现最好），按分位划分 5 档
        var sorted = values.Select((v, idx) => new { v, idx }).ToList();
        sorted = ascending
            ? sorted.OrderBy(x => x.v).ToList()
            : sorted.OrderByDescending(x => x.v).ToList();

        var scores = new int[n];
        for (var rank = 0; rank < n; rank++)
        {
            var percentile = (double)rank / n;
            scores[sorted[rank].idx] = 5 - Math.Min(4, (int)(percentile * 5));
        }

        return scores.ToList();
    }

    /// <summary>
    /// 计算中位数
    /// </summary>
    /// <param name="values">数值集合</param>
    /// <returns>中位数</returns>
    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        if (sorted.Count == 0)
        {
            return 0;
        }

        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }

    /// <summary>
    /// 按 R/F/M 高中低组合划分 8 类客户价值分群（以中位数为高低阈值）
    /// </summary>
    /// <param name="recency">最近消费天数</param>
    /// <param name="frequency">消费频次</param>
    /// <param name="monetary">消费总额</param>
    /// <param name="rMedian">R中位数</param>
    /// <param name="fMedian">F中位数</param>
    /// <param name="mMedian">M中位数</param>
    /// <returns>分群名称</returns>
    private static string GetRfmGroup(int recency, int frequency, decimal monetary, double rMedian, double fMedian, double mMedian)
    {
        var rHigh = recency <= rMedian;     // 最近消费（天数少）视为高
        var fHigh = frequency > fMedian;    // 频次高于中位数视为高
        var mHigh = (double)monetary > mMedian; // 金额高于中位数视为高

        if (rHigh && fHigh && mHigh) return "重要价值";
        if (rHigh && !fHigh && mHigh) return "重要发展";
        if (!rHigh && fHigh && mHigh) return "重要保持";
        if (!rHigh && !fHigh && mHigh) return "重要挽留";
        if (rHigh && fHigh && !mHigh) return "一般价值";
        if (rHigh && !fHigh && !mHigh) return "一般发展";
        if (!rHigh && fHigh && !mHigh) return "一般保持";
        return "流失会员";
    }

    /// <inheritdoc />
    public Task<byte[]> ExportExcel(string reportType, DateTime? dateFrom, DateTime? dateTo)
    {
        // TODO: 实现Excel导出功能
        // 使用EPPlus或NPOI等库生成Excel文件
        // 支持报表类型：营收报表、订单报表、会员报表、桌游库存报表

        return Task.FromResult(Array.Empty<byte>());
    }

    /// <summary>
    /// 内部方法：获取营收趋势数据
    /// </summary>
    private List<TrendDataPointDto> GetRevenueTrendInternal(int days)
    {
        var result = new List<TrendDataPointDto>();
        var today = DateTime.Today;

        for (var i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var nextDate = date.AddDays(1);

            var dayRevenue = _unitOfWork.Orders
                .Find(o => o.CreatedAt >= date && o.CreatedAt < nextDate && o.Status == OrderStatus.Paid)
                .Sum(o => o.ActualAmount);

            result.Add(new TrendDataPointDto
            {
                Time = date,
                Value = dayRevenue
            });
        }

        return result;
    }
}
