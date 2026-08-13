using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 会员服务接口
/// </summary>
public interface IMemberService
{
    /// <summary>
    /// 获取会员分页列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>会员分页结果</returns>
    Task<PagedResult<MemberDto>> GetPagedList(MemberQueryParams queryParams);

    /// <summary>
    /// 根据Id获取会员信息
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>会员信息DTO</returns>
    Task<MemberDto> GetById(Guid id);

    /// <summary>
    /// 创建会员
    /// </summary>
    /// <param name="dto">创建会员请求DTO</param>
    /// <returns>会员信息DTO</returns>
    Task<MemberDto> CreateAsync(CreateMemberDto dto);

    /// <summary>
    /// 更新会员资料
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <param name="dto">更新资料请求DTO</param>
    /// <returns>任务</returns>
    Task UpdateProfile(Guid id, UpdateProfileDto dto);

    /// <summary>
    /// 更新会员（管理员操作）
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <param name="dto">更新资料请求DTO</param>
    /// <returns>任务</returns>
    Task UpdateAsync(Guid id, UpdateProfileDto dto);

    /// <summary>
    /// 管理员更新会员信息（含等级、手机号）
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <param name="dto">更新会员请求DTO</param>
    /// <returns>任务</returns>
    Task UpdateMember(Guid id, UpdateMemberDto dto);

    /// <summary>
    /// 会员充值
    /// </summary>
    /// <param name="dto">充值请求DTO</param>
    /// <returns>充值记录DTO</returns>
    Task<RechargeRecordDto> Recharge(RechargeDto dto);

    /// <summary>
    /// 获取会员充值记录
    /// </summary>
    /// <param name="memberId">会员Id</param>
    /// <returns>充值记录列表</returns>
    Task<List<RechargeRecordDto>> GetRechargeRecords(Guid memberId);

    /// <summary>
    /// 获取会员消费历史
    /// </summary>
    /// <param name="memberId">会员Id</param>
    /// <returns>订单列表</returns>
    Task<List<OrderDto>> GetConsumptionHistory(Guid memberId);

    /// <summary>
    /// 获取会员余额流水（消费扣款记录）
    /// </summary>
    /// <param name="memberId">会员Id</param>
    /// <returns>余额流水列表</returns>
    Task<List<MemberBalanceRecordDto>> GetBalanceRecords(Guid memberId);
}
