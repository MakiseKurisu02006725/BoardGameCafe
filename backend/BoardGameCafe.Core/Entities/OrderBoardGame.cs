namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 订单-桌游关联实体：开台时选择的游玩桌游（一对多），结账/退款后归还库存
/// </summary>
public class OrderBoardGame
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 订单Id
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// 桌游Id
    /// </summary>
    public Guid BoardGameId { get; set; }

    /// <summary>
    /// 借出数量
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 是否已归还（结账完成/退款后置为 true，库存恢复）
    /// </summary>
    public bool IsReturned { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
