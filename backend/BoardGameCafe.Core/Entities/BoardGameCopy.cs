namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 桌游副本状态
/// </summary>
public enum CopyStatus
{
    /// <summary>
    /// 在库
    /// </summary>
    InStock = 1,

    /// <summary>
    /// 借出中
    /// </summary>
    Borrowed = 2,

    /// <summary>
    /// 已损坏
    /// </summary>
    Damaged = 3
}

/// <summary>
/// 副本借出来源类型
/// </summary>
public enum CopySource
{
    /// <summary>
    /// 手动借出（桌游管理页）
    /// </summary>
    Manual = 1,

    /// <summary>
    /// 预约借出（预约确认时自动借出）
    /// </summary>
    Reservation = 2
}

/// <summary>
/// 桌游副本实体：每个副本有独立编号，用于借出/损坏追溯
/// </summary>
public class BoardGameCopy
{
    /// <summary>
    /// 副本主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所属桌游Id
    /// </summary>
    public Guid BoardGameId { get; set; }

    /// <summary>
    /// 副本编号（拼音首字母 + 3位序号，如 CG001）
    /// </summary>
    public string CopyNumber { get; set; } = string.Empty;

    /// <summary>
    /// 副本状态（在库/借出/损坏）
    /// </summary>
    public CopyStatus Status { get; set; }

    /// <summary>
    /// 借出来源（手动借出/预约借出）
    /// </summary>
    public CopySource Source { get; set; } = CopySource.Manual;

    /// <summary>
    /// 关联预约Id（预约借出时记录，用于取消/完成/结账时自动归还）
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
