namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 仪表盘统计数据DTO
/// </summary>
public class DashboardDto
{
    /// <summary>
    /// 今日营收
    /// </summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>
    /// 今日订单数
    /// </summary>
    public int TodayOrders { get; set; }

    /// <summary>
    /// 桌位上客率（百分比0-100）
    /// </summary>
    public decimal OpenRate { get; set; }

    /// <summary>
    /// 当前在店人数
    /// </summary>
    public int InStorePeople { get; set; }

    /// <summary>
    /// 今日营业额环比（百分比；昨期为0且有今日数据时返回null表示无环比数据）
    /// </summary>
    public decimal? RevenueTrendPercent { get; set; }

    /// <summary>
    /// 今日订单数环比（百分比；昨期为0且有今日数据时返回null表示无环比数据）
    /// </summary>
    public decimal? OrdersTrendPercent { get; set; }

    /// <summary>
    /// 营收趋势数据
    /// </summary>
    public List<TrendDataPointDto> RevenueTrend { get; set; } = new();

    /// <summary>
    /// 热门桌游排行
    /// </summary>
    public List<PopularGameDto> PopularGames { get; set; } = new();
}

/// <summary>
/// 按日期统计营收DTO
/// </summary>
public class RevenueByDateDto
{
    /// <summary>
    /// 统计日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 营收金额
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// 热门桌游DTO
/// </summary>
public class PopularGameDto
{
    /// <summary>
    /// 桌游名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 使用次数
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// 会员RFM分析DTO
/// </summary>
public class MemberRfmDto
{
    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 会员昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 最近消费距今天数（Recency）
    /// </summary>
    public int Recency { get; set; }

    /// <summary>
    /// 消费频次（Frequency）
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// 消费总金额（Monetary）
    /// </summary>
    public decimal Monetary { get; set; }

    /// <summary>
    /// RFM综合评分（1-10）
    /// </summary>
    public int RfmScore { get; set; }

    /// <summary>
    /// 客户价值标签
    /// </summary>
    public string Tag { get; set; } = string.Empty;
}

/// <summary>
/// 趋势数据点DTO
/// </summary>
public class TrendDataPointDto
{
    /// <summary>
    /// 时间点
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 对应数值
    /// </summary>
    public decimal Value { get; set; }
}

/// <summary>
/// 时段客流统计DTO（按日期+小时分组）
/// </summary>
public class FlowPointDto
{
    /// <summary>
    /// 统计日期（开台日期）
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 小时（0-23）
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// 该时段客流数（订单数）
    /// </summary>
    public int Count { get; set; }
}
