using BoardGameCafe.Common;
using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 订单信息DTO
/// </summary>
public class OrderDto
{
    /// <summary>
    /// 订单主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 订单编号（如 ORD202408100003）
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// 会员名称
    /// </summary>
    public string? MemberName { get; set; }

    /// <summary>
    /// 桌位Id
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// 桌位编号
    /// </summary>
    public string? TableCode { get; set; }

    /// <summary>
    /// 游玩桌游名称（逗号拼接，列表展示用）
    /// </summary>
    public string? BoardGameNames { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 消费时长（分钟）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 折扣折数（0.1 ~ 10，10 折 = 原价无优惠）
    /// </summary>
    public decimal DiscountRate { get; set; } = 10m;

    /// <summary>
    /// 折扣金额
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// 实付金额
    /// </summary>
    public decimal ActualAmount { get; set; }

    /// <summary>
    /// 订单状态枚举值
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 订单状态名称
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 订单项DTO
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// 订单项主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 订单项类型
    /// </summary>
    public OrderItemType Type { get; set; }

    /// <summary>
    /// 小计金额
    /// </summary>
    public decimal Subtotal { get; set; }
}

/// <summary>
/// 订单详情DTO（包含订单项列表）
/// </summary>
public class OrderDetailDto : OrderDto
{
    /// <summary>
    /// 订单项列表
    /// </summary>
    public List<OrderItemDto> OrderItems { get; set; } = new();

    /// <summary>
    /// 游玩桌游列表（开台时选择的桌游）
    /// </summary>
    public List<OrderBoardGameDto> BoardGames { get; set; } = new();
}

/// <summary>
/// 订单-桌游关联DTO（订单详情展示游玩桌游）
/// </summary>
public class OrderBoardGameDto
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
    /// 是否已归还（结账/退款后为 true）
    /// </summary>
    public bool IsReturned { get; set; }
}

/// <summary>
/// 订单查询参数DTO
/// </summary>
public class OrderQueryParams : PaginationParams
{
    /// <summary>
    /// 关键词搜索（订单编号/会员名称）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 订单状态筛选
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// 查询开始日期
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// 查询结束日期
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// 会员Id筛选
    /// </summary>
    public Guid? MemberId { get; set; }
}

/// <summary>
/// 订单支付请求DTO
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// 订单Id
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 优惠券Id（可选）
    /// </summary>
    public Guid? CouponId { get; set; }

    /// <summary>
    /// 积分抵扣数量（可选）
    /// </summary>
    public int? PointsDeduct { get; set; }

    /// <summary>
    /// 余额抵扣金额（可选）
    /// </summary>
    public decimal? BalanceDeduct { get; set; }
}
