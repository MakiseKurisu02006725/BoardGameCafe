using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Infrastructure.Data;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 日志清理配置读写助手：从 SystemSettings 表读取/保存清理配置（运行时修改无需重启）
/// </summary>
public static class CleanupSettingsHelper
{
    /// <summary>
    /// 查询类日志保留天数配置键
    /// </summary>
    public const string KeyQueryDays = "LogCleanup:QueryDays";

    /// <summary>
    /// 操作类日志保留天数配置键
    /// </summary>
    public const string KeyOperationDays = "LogCleanup:OperationDays";

    /// <summary>
    /// 登录类日志保留天数配置键
    /// </summary>
    public const string KeyLoginDays = "LogCleanup:LoginDays";

    /// <summary>
    /// 自动清理开关配置键（"true"/"false"）
    /// </summary>
    public const string KeyEnabled = "LogCleanup:Enabled";

    /// <summary>
    /// 每日执行时间配置键（HH:mm）
    /// </summary>
    public const string KeyExecuteTime = "LogCleanup:ExecuteTime";

    /// <summary>
    /// 默认配置（表无记录时使用）
    /// </summary>
    private static readonly CleanupSettingsDto Defaults = new();

    /// <summary>
    /// 从数据库读取当前清理配置（缺失的键返回默认值）
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <returns>清理配置</returns>
    public static CleanupSettingsDto GetSettings(BoardGameCafeDbContext db)
    {
        var settings = db.SystemSettings.AsQueryable().ToList();
        var map = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

        return new CleanupSettingsDto
        {
            QueryDays = GetInt(map, KeyQueryDays, Defaults.QueryDays),
            OperationDays = GetInt(map, KeyOperationDays, Defaults.OperationDays),
            LoginDays = GetInt(map, KeyLoginDays, Defaults.LoginDays),
            Enabled = GetBool(map, KeyEnabled, Defaults.Enabled),
            ExecuteTime = map.TryGetValue(KeyExecuteTime, out var t) && !string.IsNullOrWhiteSpace(t) ? t : Defaults.ExecuteTime
        };
    }

    /// <summary>
    /// 将配置保存到数据库（幂等 upsert：已存在的键更新，不存在的键新增）
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="dto">清理配置</param>
    public static void SaveSettings(BoardGameCafeDbContext db, CleanupSettingsDto dto)
    {
        var now = DateTime.Now;
        var values = new (string Key, string Value, string Description)[]
        {
            (KeyQueryDays, dto.QueryDays.ToString(), "查询日志（QUERY）保留天数"),
            (KeyOperationDays, dto.OperationDays.ToString(), "操作日志（CREATE/UPDATE/DELETE）保留天数"),
            (KeyLoginDays, dto.LoginDays.ToString(), "登录登出日志（LOGIN/LOGOUT）保留天数"),
            (KeyEnabled, dto.Enabled.ToString().ToLowerInvariant(), "是否启用每日自动清理"),
            (KeyExecuteTime, dto.ExecuteTime, "每日执行清理时间（HH:mm）")
        };

        var existing = db.SystemSettings.AsQueryable().ToList();
        foreach (var (key, value, desc) in values)
        {
            var item = existing.FirstOrDefault(s => s.Key == key);
            if (item == null)
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Description = desc,
                    UpdatedAt = now
                });
            }
            else
            {
                item.Value = value;
                item.UpdatedAt = now;
                db.SystemSettings.Update(item);
            }
        }

        db.SaveChanges();
    }

    /// <summary>
    /// 读取整数配置值
    /// </summary>
    private static int GetInt(IReadOnlyDictionary<string, string> map, string key, int defaultValue)
    {
        return map.TryGetValue(key, out var v) && int.TryParse(v, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// 读取布尔配置值
    /// </summary>
    private static bool GetBool(IReadOnlyDictionary<string, string> map, string key, bool defaultValue)
    {
        return map.TryGetValue(key, out var v) && bool.TryParse(v, out var result) ? result : defaultValue;
    }
}
