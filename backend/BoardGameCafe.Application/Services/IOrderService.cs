using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 订单服务接口
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// 获取订单分页列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>订单分页结果</returns>
    Task<PagedResult<OrderDto>> GetPagedList(OrderQueryParams queryParams);

    /// <summary>
    /// 根据Id获取订单信息
    /// </summary>
    /// <param name="id">订单Id</param>
    /// <returns>订单信息DTO</returns>
    Task<OrderDto> GetById(Guid id);

    /// <summary>
    /// 订单支付
    /// </summary>
    /// <param name="dto">支付请求DTO</param>
    /// <returns>订单详情DTO</returns>
    Task<OrderDetailDto> Pay(PaymentDto dto);

    /// <summary>
    /// 订单退款
    /// </summary>
    /// <param name="id">订单Id</param>
    /// <param name="operatorId">操作人用户Id（可为空）</param>
    /// <returns>任务</returns>
    Task Refund(Guid id, Guid? operatorId);

    /// <summary>
    /// 获取订单详情（包含订单项）
    /// </summary>
    /// <param name="id">订单Id</param>
    /// <returns>订单详情DTO</returns>
    Task<OrderDetailDto> GetDetail(Guid id);
}
