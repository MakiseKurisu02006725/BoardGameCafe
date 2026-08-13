using BoardGameCafe.Common;
using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 桌游信息DTO
/// </summary>
public class BoardGameDto
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
    /// 桌游分类枚举值
    /// </summary>
    public GameCategory Category { get; set; }

    /// <summary>
    /// 桌游分类名称
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

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
    /// 桌游图片地址
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 总库存数量
    /// </summary>
    public int TotalStock { get; set; }

    /// <summary>
    /// 当前可用库存
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// 损坏库存数量
    /// </summary>
    public int DamagedStock { get; set; }

    /// <summary>
    /// 每小时租赁价格
    /// </summary>
    public decimal PricePerHour { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建桌游请求DTO
/// </summary>
public class CreateBoardGameDto
{
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
    /// 最多玩家数（可选）
    /// </summary>
    public int? MaxPlayers { get; set; }

    /// <summary>
    /// 桌游描述（可选）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 桌游图片地址（可选）
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 总库存数量
    /// </summary>
    public int TotalStock { get; set; }

    /// <summary>
    /// 每小时租赁价格
    /// </summary>
    public decimal PricePerHour { get; set; }
}

/// <summary>
/// 更新桌游请求DTO
/// </summary>
public class UpdateBoardGameDto
{
    /// <summary>
    /// 桌游名称（可选）
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 桌游分类（可选）
    /// </summary>
    public GameCategory? Category { get; set; }

    /// <summary>
    /// 最少玩家数（可选）
    /// </summary>
    public int? MinPlayers { get; set; }

    /// <summary>
    /// 最多玩家数（可选）
    /// </summary>
    public int? MaxPlayers { get; set; }

    /// <summary>
    /// 桌游描述（可选）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 桌游图片地址（可选）
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 总库存数量（可选）
    /// </summary>
    public int? TotalStock { get; set; }

    /// <summary>
    /// 损坏库存数量（可选）
    /// </summary>
    public int? DamagedStock { get; set; }

    /// <summary>
    /// 每小时租赁价格（可选）
    /// </summary>
    public decimal? PricePerHour { get; set; }
}

/// <summary>
/// 桌游查询参数DTO
/// </summary>
public class GameQueryParams : PaginationParams
{
    /// <summary>
    /// 关键词搜索（桌游名称）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 桌游分类筛选
    /// </summary>
    public GameCategory? Category { get; set; }

    /// <summary>
    /// 最少玩家数筛选
    /// </summary>
    public int? MinPlayers { get; set; }

    /// <summary>
    /// 最多玩家数筛选
    /// </summary>
    public int? MaxPlayers { get; set; }

    /// <summary>
    /// 是否仅显示有库存的桌游
    /// </summary>
    public bool? InStockOnly { get; set; }
}

/// <summary>
/// 补货请求DTO
/// </summary>
public class RestockDto
{
    /// <summary>
    /// 补货数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 补货备注（可选）
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 被借出桌游DTO（桌游管理页"借出中"列表）
/// </summary>
public class BorrowedGameDto
{
    /// <summary>
    /// 桌游Id
    /// </summary>
    public Guid BoardGameId { get; set; }

    /// <summary>
    /// 桌游名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 桌游编号
    /// </summary>
    public string GameNo { get; set; } = string.Empty;

    /// <summary>
    /// 借出数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 关联订单Id
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// 关联订单号
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 桌位编号
    /// </summary>
    public string? TableCode { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatus OrderStatus { get; set; }
}
