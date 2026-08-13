using BoardGameCafe.Application.DTOs;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 桌位服务接口
/// </summary>
public interface ITableService
{
    /// <summary>
    /// 获取所有桌位
    /// </summary>
    /// <returns>桌位列表</returns>
    Task<List<TableDto>> GetAll();

    /// <summary>
    /// 根据Id获取桌位信息
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <returns>桌位信息DTO</returns>
    Task<TableDto> GetById(Guid id);

    /// <summary>
    /// 创建桌位
    /// </summary>
    /// <param name="dto">创建桌位请求DTO</param>
    /// <returns>桌位信息DTO</returns>
    Task<TableDto> Create(CreateTableDto dto);

    /// <summary>
    /// 更新桌位信息
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <param name="dto">更新桌位请求DTO</param>
    /// <returns>任务</returns>
    Task Update(Guid id, UpdateTableDto dto);

    /// <summary>
    /// 删除桌位
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <returns>任务</returns>
    Task Delete(Guid id);

    /// <summary>
    /// 开台
    /// </summary>
    /// <param name="dto">开台请求DTO</param>
    /// <param name="createdBy">创建人Id</param>
    /// <returns>订单信息DTO</returns>
    Task<OrderDto> OpenTable(OpenTableDto dto, Guid createdBy);

    /// <summary>
    /// 换台
    /// </summary>
    /// <param name="dto">换台请求DTO</param>
    /// <returns>任务</returns>
    Task ChangeTable(ChangeTableDto dto);

    /// <summary>
    /// 结账关台（计算费用）
    /// </summary>
    /// <param name="dto">关台请求DTO</param>
    /// <returns>订单详情DTO</returns>
    Task<OrderDetailDto> CloseTable(CloseTableDto dto);

    /// <summary>
    /// 设置/解除桌位维修状态
    /// </summary>
    /// <param name="id">桌位Id</param>
    /// <param name="status">maintain（设置维修）/ normal（解除维修）</param>
    /// <param name="operatorId">操作人用户Id</param>
    /// <returns>任务</returns>
    Task Maintain(Guid id, string status, Guid? operatorId);
}
