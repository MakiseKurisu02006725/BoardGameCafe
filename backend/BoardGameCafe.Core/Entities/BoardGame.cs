using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 桌游实体
/// </summary>
public class BoardGame
{
    /// <summary>
    /// 桌游主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 桌游编号（如 B0077）
    /// </summary>
    public string GameNo { get; set; } = string.Empty;

    /// <summary>
    /// 桌游名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 桌游分类
    /// </summary>
    public GameCategory Category { get; set; }

    /// <summary>
    /// 最少玩家数
    /// </summary>
    public int MinPlayers { get; set; }

    /// <summary>
    /// 最多玩家数
    /// </summary>
    public int MaxPlayers { get; set; }

    /// <summary>
    /// 桌游描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 总库存
    /// </summary>
    public int TotalStock { get; set; }

    /// <summary>
    /// 当前库存
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// 损坏库存
    /// </summary>
    public int DamagedStock { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 每小时价格
    /// </summary>
    public decimal PricePerHour { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
