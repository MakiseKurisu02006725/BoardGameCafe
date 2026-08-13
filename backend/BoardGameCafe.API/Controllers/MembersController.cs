using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using System.Security.Claims;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 会员管理控制器
/// </summary>
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="memberService">会员服务</param>
    /// <param name="unitOfWork">工作单元</param>
    public MembersController(IMemberService memberService, IUnitOfWork unitOfWork)
    {
        _memberService = memberService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 分页查询会员列表
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>会员分页列表</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(PagedApiResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedApiResponse<MemberDto>>> GetMembers([FromQuery] MemberQueryParams queryParams)
    {
        var result = await _memberService.GetPagedList(queryParams);
        return Ok(PagedApiResponse<MemberDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取会员详情
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>会员详情</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MemberDto>>> GetMemberById([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();

        if (!isStaff)
        {
            // 会员只能查看自己的信息，通过 JWT 中的 userId 查找对应的会员记录
            var userGuid = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            var member = _unitOfWork.Members.Find(m => m.UserId == userGuid).FirstOrDefault();
            if (member == null)
            {
                return NotFound(ApiResponse.Fail("会员不存在", 404));
            }
            if (member.Id != id)
            {
                return Forbid();
            }
        }

        var result = await _memberService.GetById(id);
        return Ok(ApiResponse<MemberDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 修改个人信息
    /// </summary>
    /// <param name="dto">更新资料请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPut("profile")]
    [Authorize(Roles = "Member,Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        await _memberService.UpdateProfile(userId, dto);
        return Ok(ApiResponse.Success("修改成功"));
    }

    /// <summary>
    /// 创建会员（管理员）
    /// </summary>
    /// <param name="dto">创建会员请求参数</param>
    /// <returns>会员信息</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<MemberDto>>> CreateMember([FromBody] CreateMemberDto dto)
    {
        var result = await _memberService.CreateAsync(dto);
        return Ok(ApiResponse<MemberDto>.Success(result, "创建成功"));
    }

    /// <summary>
    /// 更新会员信息（管理员）
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <param name="dto">更新会员请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> UpdateMember([FromRoute] Guid id, [FromBody] UpdateMemberDto dto)
    {
        await _memberService.UpdateMember(id, dto);
        return Ok(ApiResponse.Success("更新成功"));
    }

    /// <summary>
    /// 会员充值
    /// </summary>
    /// <param name="dto">充值请求参数</param>
    /// <returns>充值记录</returns>
    [HttpPost("recharge")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<RechargeRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<RechargeRecordDto>>> Recharge([FromBody] RechargeDto dto)
    {
        var result = await _memberService.Recharge(dto);
        return Ok(ApiResponse<RechargeRecordDto>.Success(result, "充值成功"));
    }

    /// <summary>
    /// 删除会员
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteMember([FromRoute] Guid id)
    {
        var member = await _memberService.GetById(id);
        if (member == null)
        {
            return NotFound(ApiResponse.Fail("会员不存在", 404));
        }

        // 软删除
        var existingMember = _unitOfWork.Members.GetById(id);
        if (existingMember != null)
        {
            _unitOfWork.Members.Delete(existingMember);
            _unitOfWork.SaveChanges();
        }

        return Ok(ApiResponse.Success("删除成功"));
    }

    /// <summary>
    /// 获取会员充值记录
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>充值记录列表</returns>
    [HttpGet("{id}/recharges")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<RechargeRecordDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<RechargeRecordDto>>>> GetRecharges([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();

        if (!isStaff)
        {
            var userGuid = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            var member = _unitOfWork.Members.Find(m => m.UserId == userGuid).FirstOrDefault();
            if (member == null || member.Id != id)
            {
                return Forbid();
            }
        }

        var result = await _memberService.GetRechargeRecords(id);
        return Ok(ApiResponse<List<RechargeRecordDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取会员余额流水（消费扣款记录）
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>余额流水列表</returns>
    [HttpGet("{id}/balance-records")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<MemberBalanceRecordDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<MemberBalanceRecordDto>>>> GetBalanceRecords([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();

        if (!isStaff)
        {
            var userGuid = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            var member = _unitOfWork.Members.Find(m => m.UserId == userGuid).FirstOrDefault();
            if (member == null || member.Id != id)
            {
                return Forbid();
            }
        }

        var result = await _memberService.GetBalanceRecords(id);
        return Ok(ApiResponse<List<MemberBalanceRecordDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取会员消费明细
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>消费明细列表</returns>
    [HttpGet("{id}/orders")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrders([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();

        if (!isStaff)
        {
            var userGuid = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            var member = _unitOfWork.Members.Find(m => m.UserId == userGuid).FirstOrDefault();
            if (member == null || member.Id != id)
            {
                return Forbid();
            }
        }

        var result = await _memberService.GetConsumptionHistory(id);
        return Ok(ApiResponse<List<OrderDto>>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取会员优惠券
    /// </summary>
    /// <param name="id">会员Id</param>
    /// <returns>优惠券列表</returns>
    [HttpGet("{id}/coupons")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<MemberCouponDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<List<MemberCouponDto>>> GetCoupons([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();

        if (!isStaff)
        {
            var userGuid = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            var member = _unitOfWork.Members.Find(m => m.UserId == userGuid).FirstOrDefault();
            if (member == null || member.Id != id)
            {
                return Forbid();
            }
        }

        var coupons = _unitOfWork.Coupons.Find(c => c.MemberId == id).ToList();
        var dtos = coupons.Select(c => new MemberCouponDto
        {
            Id = c.Id,
            CouponId = c.Id,
            CouponName = c.Name,
            Type = c.Discount.HasValue ? "折扣" : "满减",
            DiscountValue = c.DiscountAmount ?? (c.Discount ?? 0) * 10,
            MinAmount = c.MinAmount,
            Status = c.IsUsed ? "已使用" : "未使用",
            ValidFrom = c.CreatedAt,
            ValidTo = c.ExpireDate
        }).ToList();

        return Ok(ApiResponse<List<MemberCouponDto>>.Success(dtos, "查询成功"));
    }
}
