using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 修改桌位维修状态请求DTO
/// </summary>
public class MaintainTableDto
{
    /// <summary>
    /// 目标状态：maintain（设置维修）/ normal（解除维修）
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 桌位信息DTO
/// </summary>
public class TableDto
{
    /// <summary>
    /// 桌位主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 桌位编号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 桌位区域枚举值
    /// </summary>
    public TableArea Area { get; set; }

    /// <summary>
    /// 桌位区域名称
    /// </summary>
    public string AreaName { get; set; } = string.Empty;

    /// <summary>
    /// 座位数
    /// </summary>
    public int Seats { get; set; }

    /// <summary>
    /// 桌位状态枚举值
    /// </summary>
    public TableStatus Status { get; set; }

    /// <summary>
    /// 桌位状态名称
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// 每小时费率
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// 关联预约Id（仅已预约桌位返回，空闲/占用时为 null）
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// 预约联系人（会员昵称，已预约桌位显示）
    /// </summary>
    public string ReservedName { get; set; } = string.Empty;

    /// <summary>
    /// 预约人数（已预约桌位显示）
    /// </summary>
    public int PeopleCount { get; set; }

    /// <summary>
    /// 预约时间段（已预约桌位返回，如 10:00-11:00）
    /// </summary>
    public string ReservedTime { get; set; } = string.Empty;

    /// <summary>
    /// 当前待支付订单Id（占用中桌位返回，用于结账）
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 会员名称（占用中桌位返回）
    /// </summary>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// 开台时间（占用中桌位返回，用于显示已用时长）
    /// </summary>
    public DateTime? OpenTime { get; set; }
}

/// <summary>
/// 创建桌位请求DTO
/// </summary>
public class CreateTableDto
{
    /// <summary>
    /// 桌位编号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 桌位区域
    /// </summary>
    public TableArea Area { get; set; }

    /// <summary>
    /// 座位数
    /// </summary>
    public int Seats { get; set; }

    /// <summary>
    /// 每小时费率
    /// </summary>
    public decimal HourlyRate { get; set; }
}

/// <summary>
/// 更新桌位请求DTO
/// </summary>
public class UpdateTableDto
{
    /// <summary>
    /// 桌位编号（可选）
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 桌位区域（可选）
    /// </summary>
    public TableArea? Area { get; set; }

    /// <summary>
    /// 座位数（可选）
    /// </summary>
    public int? Seats { get; set; }

    /// <summary>
    /// 每小时费率（可选）
    /// </summary>
    public decimal? HourlyRate { get; set; }

    /// <summary>
    /// 桌位状态（可选）
    /// </summary>
    public TableStatus? Status { get; set; }
}

/// <summary>
/// 开台请求DTO
/// </summary>
public class OpenTableDto
{
    /// <summary>
    /// 桌位Id
    /// </summary>
    public Guid TableId { get; set; }

    /// <summary>
    /// 会员Id（可选，非会员消费则为空）
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// 客人数量
    /// </summary>
    public int GuestCount { get; set; }

    /// <summary>
    /// 折扣折数（可选，0.1 ~ 10，默认 10 折 = 原价）
    /// </summary>
    public decimal? Discount { get; set; }

    /// <summary>
    /// 游玩桌游Id列表（必选，至少选择一款，结账/退款后自动归还库存）
    /// </summary>
    public List<Guid>? BoardGameIds { get; set; }
}

/// <summary>
/// 换台请求DTO
/// </summary>
public class ChangeTableDto
{
    /// <summary>
    /// 当前订单Id
    /// </summary>
    public Guid CurrentOrderId { get; set; }

    /// <summary>
    /// 新桌位Id
    /// </summary>
    public Guid NewTableId { get; set; }
}

/// <summary>
/// 结账关台请求DTO
/// </summary>
public class CloseTableDto
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
