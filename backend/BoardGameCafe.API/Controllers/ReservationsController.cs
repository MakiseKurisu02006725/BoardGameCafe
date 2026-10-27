using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Common;
using BoardGameCafe.Common.Exceptions;
using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using System.Security.Claims;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 预约管理控制器
/// </summary>
[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationsController(IReservationService reservationService, IUnitOfWork unitOfWork)
    {
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 分页查询预约
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns>预约分页列表</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedApiResponse<ReservationDto>>> GetReservations([FromQuery] ReservationQueryParams queryParams)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();

        if (!isStaff)
        {
            // 会员只能查看自己的预约：通过 userId 解析真实会员Id（MemberId 是 Member 表主键，不能用 User.Id）
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var member = _unitOfWork.Members.Find(m => m.UserId == userId).FirstOrDefault();
                if (member != null)
                {
                    queryParams.MemberId = member.Id;
                }
                else
                {
                    // 会员账号未关联会员记录时返回空列表
                    queryParams.MemberId = Guid.Empty;
                }
            }
        }

        var result = await _reservationService.GetPagedList(queryParams);
        return Ok(PagedApiResponse<ReservationDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 获取预约详情
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>预约详情</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetReservationById([FromRoute] Guid id)
    {
        var result = await _reservationService.GetById(id);

        // 防水平越权：会员只能查看自己的预约
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();
        if (!isStaff)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var member = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? _unitOfWork.Members.Find(m => m.UserId == userId).FirstOrDefault()
                : null;
            if (member == null || result.MemberId != member.Id)
            {
                return NotFound(ApiResponse.Fail("预约不存在", 404));
            }
        }

        return Ok(ApiResponse<ReservationDto>.Success(result, "查询成功"));
    }

    /// <summary>
    /// 创建预约（会员）
    /// </summary>
    /// <param name="request">创建预约请求参数（前端格式）</param>
    /// <returns>预约详情</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> CreateReservation([FromBody] CreateReservationRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isMemberRole = roleClaim != null && roleClaim.Value == UserRole.Member.ToString();

        var memberId = ResolveMemberId(userId, isMemberRole, request.Phone, request.NickName);

        if (!TryParseTableType(request.TableType, out var tableType))
        {
            return BadRequest(ApiResponse.Fail("无效的桌位类型", 400));
        }

        if (!DateOnly.TryParse(request.Date, out var date))
        {
            return BadRequest(ApiResponse.Fail("日期格式错误", 400));
        }

        if (!TimeSpan.TryParse(request.StartTime, out var startTime))
        {
            return BadRequest(ApiResponse.Fail("开始时间格式错误", 400));
        }

        if (!TimeSpan.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest(ApiResponse.Fail("结束时间格式错误", 400));
        }

        var dto = new CreateReservationDto
        {
            TableId = request.TableId,
            GameIds = request.GameIds ?? new List<Guid>(),
            TableType = tableType,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            GuestCount = request.PeopleCount,
            ContactPhone = request.Phone,
            Remark = request.Remark
        };

        var result = await _reservationService.Create(dto, memberId);
        return Ok(ApiResponse<ReservationDto>.Success(result, "预约成功"));
    }

    private Guid ResolveMemberId(Guid userId, bool isMemberRole, string? phone, string? nickName)
    {
        if (isMemberRole)
        {
            var member = _unitOfWork.Members.Find(m => m.UserId == userId).FirstOrDefault();
            if (member != null)
                return member.Id;
        }

        if (!string.IsNullOrEmpty(phone))
        {
            var user = _unitOfWork.Users.Find(u => u.Phone == phone).FirstOrDefault();
            if (user != null)
            {
                var member = _unitOfWork.Members.Find(m => m.UserId == user.Id).FirstOrDefault();
                if (member != null)
                    return member.Id;

                var existingUserMember = CreateMemberForExistingUser(user, nickName);
                return existingUserMember.Id;
            }

            var walkInMember = CreateWalkInMember(nickName ?? "散客", phone, null);
            return walkInMember.Id;
        }

        var fallback = _unitOfWork.Members.GetAll().FirstOrDefault();
        if (fallback != null)
            return fallback.Id;

        throw new BusinessException("系统中没有可用的会员，请先创建会员或提供联系电话");
    }

    private Member CreateMemberForExistingUser(User user, string? nickName)
    {
        var now = DateTime.Now;
        var member = new Member
        {
            Id = Guid.NewGuid(),
            MemberNo = BusinessNoGenerator.NextMemberNo(_unitOfWork.Members.GetAll().Select(m => m.MemberNo)),
            UserId = user.Id,
            NickName = nickName ?? user.UserName,
            Level = MemberLevel.Bronze,
            Points = 0,
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        _unitOfWork.Members.Add(member);
        _unitOfWork.SaveChanges();
        return member;
    }

    private Member CreateWalkInMember(string nickName, string phone, string? email)
    {
        var now = DateTime.Now;
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = nickName,
            PasswordHash = PasswordHelper.HashPassword("123456"),
            Phone = phone,
            Email = email,
            Role = UserRole.Member,
            CreatedAt = now,
            IsDeleted = false
        };
        _unitOfWork.Users.Add(user);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            MemberNo = BusinessNoGenerator.NextMemberNo(_unitOfWork.Members.GetAll().Select(m => m.MemberNo)),
            UserId = user.Id,
            NickName = nickName,
            Level = MemberLevel.Bronze,
            Points = 0,
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        _unitOfWork.Members.Add(member);
        _unitOfWork.SaveChanges();
        return member;
    }

    /// <summary>
    /// 解析桌位区域类型字符串
    /// </summary>
    private static bool TryParseTableType(string? value, out TableArea tableType)
    {
        tableType = TableArea.Hall;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "hall":
            case "1":
                tableType = TableArea.Hall;
                return true;
            case "room":
            case "2":
                tableType = TableArea.PrivateRoom;
                return true;
            case "booth":
            case "3":
                tableType = TableArea.Booth;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 确认预约
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> Confirm([FromRoute] Guid id)
    {
        await _reservationService.Confirm(id);
        return Ok(ApiResponse.Success("确认成功"));
    }

    /// <summary>
    /// 取消预约（本人或员工）
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> Cancel([FromRoute] Guid id)
    {
        // 防水平越权：会员只能取消自己的预约
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var isStaff = roleClaim != null && roleClaim.Value != UserRole.Member.ToString();
        if (!isStaff)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var member = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? _unitOfWork.Members.Find(m => m.UserId == userId).FirstOrDefault()
                : null;
            var reservation = _unitOfWork.Reservations.GetById(id);
            if (member == null || reservation == null || reservation.MemberId != member.Id)
            {
                return NotFound(ApiResponse.Fail("预约不存在", 404));
            }
        }

        await _reservationService.Cancel(id);
        return Ok(ApiResponse.Success("取消成功"));
    }

    /// <summary>
    /// 完成预约（已确认 → 已完成，自动归还关联桌游并释放桌位）
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> Complete([FromRoute] Guid id)
    {
        await _reservationService.Complete(id);
        return Ok(ApiResponse.Success("预约已完成"));
    }

    /// <summary>
    /// 转开台
    /// </summary>
    /// <param name="id">预约Id</param>
    /// <param name="tableId">桌位Id（为null时自动分配）</param>
    /// <returns>订单详情</returns>
    [HttpPost("{id}/convert")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ConvertToOrder([FromRoute] Guid id, [FromQuery] Guid? tableId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("用户未认证", 401));
        }

        var result = await _reservationService.ConvertToOrder(id, tableId, userId);
        return Ok(ApiResponse<OrderDto>.Success(result, "转开台成功"));
    }
}
