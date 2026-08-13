namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 损坏记录DTO
/// </summary>
public class DamageRecordDto
{
    /// <summary>
    /// 损坏记录主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 桌游Id
    /// </summary>
    public Guid BoardGameId { get; set; }

    /// <summary>
    /// 桌游名称
    /// </summary>
    public string BoardGameName { get; set; } = string.Empty;

    /// <summary>
    /// 关联订单Id
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 损坏数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 损坏原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 赔偿金额
    /// </summary>
    public decimal Compensation { get; set; }

    /// <summary>
    /// 创建人Id
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// 创建人名称
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建损坏记录请求DTO
/// </summary>
public class CreateDamageDto
{
    /// <summary>
    /// 桌游Id
    /// </summary>
    public Guid BoardGameId { get; set; }

    /// <summary>
    /// 关联订单Id（可选）
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 损坏数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 损坏原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 赔偿金额
    /// </summary>
    public decimal? Compensation { get; set; }
}
