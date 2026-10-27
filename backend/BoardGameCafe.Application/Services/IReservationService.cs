using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 预约服务接口
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// 获取预约分页列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>预约分页结果</returns>
    Task<PagedResult<ReservationDto>> GetPagedList(ReservationQueryParams queryParams);

    /// <summary>
    /// 获取预约详情
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>预约信息DTO</returns>
    Task<ReservationDto> GetById(Guid id);

    /// <summary>
    /// 创建预约（冲突检测）
    /// </summary>
    /// <param name="dto">创建预约请求DTO</param>
    /// <param name="memberId">会员Id</param>
    /// <returns>预约信息DTO</returns>
    Task<ReservationDto> Create(CreateReservationDto dto, Guid memberId);

    /// <summary>
    /// 确认预约
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>任务</returns>
    Task Confirm(Guid id);

    /// <summary>
    /// 取消预约
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>任务</returns>
    Task Cancel(Guid id);

    /// <summary>
    /// 完成预约（已确认 → 已完成，自动归还关联桌游并释放桌位）
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>任务</returns>
    Task Complete(Guid id);

    /// <summary>
    /// 预约转订单（到店开台）
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <param name="tableId">桌位Id（为null时自动分配）</param>
    /// <param name="createdBy">创建人Id</param>
    /// <returns>订单信息DTO</returns>
    Task<OrderDto> ConvertToOrder(Guid id, Guid? tableId, Guid createdBy);
}
