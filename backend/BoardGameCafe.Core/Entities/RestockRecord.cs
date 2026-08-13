namespace BoardGameCafe.Core.Entities;

/// <summary>
/// 补货记录实体
/// </summary>
public class RestockRecord
{
    /// <summary>
    /// 补货记录主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 桌游Id
    /// </summary>
    public Guid BoardGameId { get; set; }

    /// <summary>
    /// 补货数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 补货备注（可选）
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建人Id
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
