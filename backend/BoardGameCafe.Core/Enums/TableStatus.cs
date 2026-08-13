namespace BoardGameCafe.Core.Enums;

/// <summary>
/// 桌位状态
/// </summary>
public enum TableStatus
{
    /// <summary>
    /// 空闲
    /// </summary>
    Idle = 1,

    /// <summary>
    /// 占用中
    /// </summary>
    Occupied = 2,

    /// <summary>
    /// 已预约
    /// </summary>
    Reserved = 3,

    /// <summary>
    /// 维护中
    /// </summary>
    Maintenance = 4
}
