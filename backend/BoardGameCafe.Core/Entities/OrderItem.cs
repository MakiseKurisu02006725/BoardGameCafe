using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 订单项实体
/// </summary>
public class OrderItem
{
    /// <summary>
    /// 订单项主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联订单Id
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// 桌游Id（可为空，非桌游类订单项）
    /// </summary>
    public Guid? BoardGameId { get; set; }

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

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
