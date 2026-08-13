using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 桌游服务接口
/// </summary>
public interface IBoardGameService
{
    /// <summary>
    /// 获取桌游分页列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>桌游分页结果</returns>
    Task<PagedResult<BoardGameDto>> GetPagedList(GameQueryParams queryParams);

    /// <summary>
    /// 根据Id获取桌游信息
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <returns>桌游信息DTO</returns>
    Task<BoardGameDto> GetById(Guid id);

    /// <summary>
    /// 创建桌游
    /// </summary>
    /// <param name="dto">创建桌游请求DTO</param>
    /// <returns>桌游信息DTO</returns>
    Task<BoardGameDto> Create(CreateBoardGameDto dto);

    /// <summary>
    /// 更新桌游信息
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="dto">更新桌游请求DTO</param>
    /// <returns>任务</returns>
    Task Update(Guid id, UpdateBoardGameDto dto);

    /// <summary>
    /// 删除桌游（软删除）
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <returns>任务</returns>
    Task Delete(Guid id);

    /// <summary>
    /// 借出桌游（减少当前库存，返回分配的副本编号）
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="quantity">借出数量</param>
    /// <returns>分配的副本编号列表</returns>
    Task<List<string>> Borrow(Guid id, int quantity);

    /// <summary>
    /// 归还桌游（增加当前库存）
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="quantity">归还数量</param>
    /// <returns>任务</returns>
    Task Return(Guid id, int quantity);

    /// <summary>
    /// 补货桌游（总库存与当前库存同时增加，优先修复损坏副本）
    /// </summary>
    /// <param name="id">桌游Id</param>
    /// <param name="dto">补货请求DTO</param>
    /// <returns>任务</returns>
    Task Restock(Guid id, RestockDto dto);

    /// <summary>
    /// 记录损坏
    /// </summary>
    /// <param name="dto">创建损坏记录DTO</param>
    /// <param name="createdBy">创建人Id</param>
    /// <returns>损坏记录DTO</returns>
    Task<DamageRecordDto> RecordDamage(CreateDamageDto dto, Guid createdBy);

    /// <summary>
    /// 获取当前借出中的桌游列表（开台/预约借出，未归还，含关联订单号）
    /// </summary>
    /// <returns>借出中桌游列表</returns>
    Task<List<BorrowedGameDto>> GetBorrowed();
}
