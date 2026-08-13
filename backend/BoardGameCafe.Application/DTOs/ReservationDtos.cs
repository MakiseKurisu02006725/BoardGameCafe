using BoardGameCafe.Common;
using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.DTOs;

/// <summary>
/// 预约信息DTO
/// </summary>
public class ReservationDto
{
    /// <summary>
    /// 预约主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 预约编号（如 RESV001）
    /// </summary>
    public string ReservationNo { get; set; } = string.Empty;

    /// <summary>
    /// 会员Id
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 会员名称
    /// </summary>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// 桌位Id
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// 桌位编号
    /// </summary>
    public string? TableCode { get; set; }

    /// <summary>
    /// 预约桌游名称（逗号分隔）
    /// </summary>
    public string? ReservedGames { get; set; }

    /// <summary>
    /// 桌位区域类型
    /// </summary>
    public TableArea TableType { get; set; }

    /// <summary>
    /// 桌位区域类型名称
    /// </summary>
    public string TableTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 预约日期
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// 客人数量
    /// </summary>
    public int GuestCount { get; set; }

    /// <summary>
    /// 押金金额
    /// </summary>
    public decimal Deposit { get; set; }

    /// <summary>
    /// 预约状态
    /// </summary>
    public ReservationStatus Status { get; set; }

    /// <summary>
    /// 预约状态名称
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// 联系电话
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建预约请求DTO
/// </summary>
public class CreateReservationDto
{
    /// <summary>
    /// 桌位Id（可选）
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// 桌游Id列表（可选，预约桌游）
    /// </summary>
    public List<Guid> GameIds { get; set; } = new();

    /// <summary>
    /// 桌位区域类型
    /// </summary>
    public TableArea TableType { get; set; }

    /// <summary>
    /// 预约日期
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// 客人数量
    /// </summary>
    public int GuestCount { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// 备注（可选）
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 预约查询参数DTO
/// </summary>
public class ReservationQueryParams : PaginationParams
{
    /// <summary>
    /// 关键字搜索
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 预约状态筛选
    /// </summary>
    public ReservationStatus? Status { get; set; }

    /// <summary>
    /// 会员Id筛选
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// 查询开始日期
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// 查询结束日期
    /// </summary>
    public DateTime? DateTo { get; set; }
}

/// <summary>
/// 创建预约请求DTO（前端表单格式）
/// </summary>
public class CreateReservationRequestDto
{
    /// <summary>
    /// 桌位Id（可选）
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// 预约桌游Id列表（可选）
    /// </summary>
    public List<Guid> GameIds { get; set; } = new();

    /// <summary>
    /// 桌位区域类型（字符串或数字：hall/room/booth 或 1/2/3）
    /// </summary>
    public string TableType { get; set; } = string.Empty;

    /// <summary>
    /// 预约日期（格式：YYYY-MM-DD）
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间（格式：HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 结束时间（格式：HH:mm）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 客人数量
    /// </summary>
    public int PeopleCount { get; set; }

    /// <summary>
    /// 会员昵称（散客时使用，可空）
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}
