namespace BoardGameCafe.Common.Helpers;

/// <summary>
/// 业务编号生成器，用于生成订单号、会员号、桌游编号、预约号等人类可读的业务编号
/// </summary>
public static class BusinessNoGenerator
{
    /// <summary>
    /// 生成订单号：ORD + yyyyMMdd + 3位当日序号（如 ORD202408100003）
    /// </summary>
    /// <param name="todayOrderNos">当天已有订单号集合</param>
    /// <returns>新订单号</returns>
    public static string NextOrderNo(IEnumerable<string> todayOrderNos)
    {
        var prefix = "ORD" + DateTime.Now.ToString("yyyyMMdd");
        var maxSeq = ParseMaxSeq(todayOrderNos, prefix);
        return $"{prefix}{(maxSeq + 1):D3}";
    }

    /// <summary>
    /// 生成会员号：M + 序号（如 M128）
    /// </summary>
    /// <param name="existingNos">现有会员号集合</param>
    /// <returns>新会员号</returns>
    public static string NextMemberNo(IEnumerable<string> existingNos)
    {
        var maxSeq = ParseMaxSeq(existingNos, "M");
        return $"M{maxSeq + 1}";
    }

    /// <summary>
    /// 生成桌游编号：B + 4位序号（如 B0077）
    /// </summary>
    /// <param name="existingNos">现有桌游编号集合</param>
    /// <returns>新桌游编号</returns>
    public static string NextGameNo(IEnumerable<string> existingNos)
    {
        var maxSeq = ParseMaxSeq(existingNos, "B");
        return $"B{(maxSeq + 1):D4}";
    }

    /// <summary>
    /// 生成预约号：RESV + 3位序号（如 RESV001）
    /// </summary>
    /// <param name="existingNos">现有预约号集合</param>
    /// <returns>新预约号</returns>
    public static string NextReservationNo(IEnumerable<string> existingNos)
    {
        var maxSeq = ParseMaxSeq(existingNos, "RESV");
        return $"RESV{(maxSeq + 1):D3}";
    }

    /// <summary>
    /// 解析指定前缀编号中的最大数字序号
    /// </summary>
    /// <param name="nos">编号集合</param>
    /// <param name="prefix">编号前缀</param>
    /// <returns>最大数字序号（无匹配时为0）</returns>
    private static int ParseMaxSeq(IEnumerable<string> nos, string prefix)
    {
        return nos
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && n.Length > prefix.Length)
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
    }
}
