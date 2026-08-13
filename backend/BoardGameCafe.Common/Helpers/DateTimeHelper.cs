namespace BoardGameCafe.Common.Helpers;

/// <summary>
/// 日期时间帮助类
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// 获取当前中国标准时间（UTC+8）
    /// </summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChinaStandardTimeZone);

    /// <summary>
    /// 获取当前中国标准时间的日期部分
    /// </summary>
    public static DateTime Today => Now.Date;

    /// <summary>
    /// 获取当前Unix时间戳（秒级）
    /// </summary>
    public static long NowUnixTimestampSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    /// 获取当前Unix时间戳（毫秒级）
    /// </summary>
    public static long NowUnixTimestampMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 中国标准时区
    /// </summary>
    private static TimeZoneInfo ChinaStandardTimeZone
    {
        get
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
                }
                catch
                {
                    return TimeZoneInfo.Local;
                }
            }
        }
    }

    /// <summary>
    /// 将DateTime转换为Unix时间戳（秒级）
    /// </summary>
    /// <param name="dateTime">待转换的日期时间</param>
    /// <returns>Unix时间戳（秒）</returns>
    public static long ToUnixTimestampSeconds(DateTime dateTime)
    {
        var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
        return dateTimeOffset.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 将DateTime转换为Unix时间戳（毫秒级）
    /// </summary>
    /// <param name="dateTime">待转换的日期时间</param>
    /// <returns>Unix时间戳（毫秒）</returns>
    public static long ToUnixTimestampMilliseconds(DateTime dateTime)
    {
        var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
        return dateTimeOffset.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 将Unix时间戳（秒级）转换为DateTime
    /// </summary>
    /// <param name="timestamp">Unix时间戳（秒）</param>
    /// <returns>转换后的DateTime（本地时间）</returns>
    public static DateTime FromUnixTimestampSeconds(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
    }

    /// <summary>
    /// 将Unix时间戳（毫秒级）转换为DateTime
    /// </summary>
    /// <param name="timestamp">Unix时间戳（毫秒）</param>
    /// <returns>转换后的DateTime（本地时间）</returns>
    public static DateTime FromUnixTimestampMilliseconds(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
    }

    /// <summary>
    /// 获取指定日期所在周的第一天（周一）
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns>所在周的周一日期</returns>
    public static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// 获取指定日期所在周的最后一天（周日）
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns>所在周的周日日期</returns>
    public static DateTime GetWeekEnd(DateTime date)
    {
        return GetWeekStart(date).AddDays(6).Date;
    }

    /// <summary>
    /// 获取指定日期所在月份的第一天
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns>所在月份的第一天</returns>
    public static DateTime GetMonthStart(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// 获取指定日期所在月份的最后一天
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns>所在月份的最后一天</returns>
    public static DateTime GetMonthEnd(DateTime date)
    {
        return GetMonthStart(date).AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// 计算两个日期之间的年龄
    /// </summary>
    /// <param name="birthDate">出生日期</param>
    /// <param name="currentDate">当前日期（默认为今天）</param>
    /// <returns>年龄</returns>
    public static int CalculateAge(DateTime birthDate, DateTime? currentDate = null)
    {
        var now = currentDate ?? Today;
        var age = now.Year - birthDate.Year;

        if (birthDate.Date > now.AddYears(-age))
        {
            age--;
        }

        return age < 0 ? 0 : age;
    }

    /// <summary>
    /// 判断指定日期是否为工作日（周一至周五）
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns>如果是工作日返回true，否则返回false</returns>
    public static bool IsWorkDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    /// <summary>
    /// 判断指定日期是否为周末
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns>如果是周末返回true，否则返回false</returns>
    public static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// 获取两个日期之间的工作日天数
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>工作日天数</returns>
    public static int GetWorkDaysCount(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        var workDays = 0;
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            if (IsWorkDay(currentDate))
            {
                workDays++;
            }

            currentDate = currentDate.AddDays(1);
        }

        return workDays;
    }

    /// <summary>
    /// 格式化日期时间为标准字符串（yyyy-MM-dd HH:mm:ss）
    /// </summary>
    /// <param name="dateTime">待格式化的日期时间</param>
    /// <returns>格式化后的字符串</returns>
    public static string FormatStandard(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 格式化日期为标准字符串（yyyy-MM-dd）
    /// </summary>
    /// <param name="date">待格式化的日期</param>
    /// <returns>格式化后的字符串</returns>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 格式化时间为标准字符串（HH:mm:ss）
    /// </summary>
    /// <param name="dateTime">待格式化的时间</param>
    /// <returns>格式化后的字符串</returns>
    public static string FormatTime(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }
}
