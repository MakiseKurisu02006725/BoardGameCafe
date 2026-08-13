using AutoMapper;
using AutoMapper.QueryableExtensions;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;
using BoardGameCafe.Common.Exceptions;
using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 会员服务实现
/// </summary>
public class MemberService : IMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MemberService> _logger;

    /// <summary>
    /// 初始化会员服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public MemberService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MemberService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PagedResult<MemberDto>> GetPagedList(MemberQueryParams queryParams)
    {
        var allMembers = _unitOfWork.Members.GetAll().AsQueryable();

        if (queryParams.Level.HasValue)
        {
            allMembers = allMembers.Where(m => m.Level == queryParams.Level.Value);
        }

        if (!string.IsNullOrEmpty(queryParams.Keyword))
        {
            var kw = queryParams.Keyword.Trim();
            // 手机号匹配（手机号存于 User 表，经 Member.UserId 关联）→ 会员Id集合
            var phoneUserIds = _unitOfWork.Users
                .Find(u => u.Phone != null && u.Phone.Contains(kw))
                .Select(u => u.Id)
                .ToHashSet();
            var phoneMemberIds = _unitOfWork.Members.GetAll()
                .Where(m => phoneUserIds.Contains(m.UserId))
                .Select(m => m.Id)
                .ToHashSet();

            // 昵称模糊匹配 或 手机号匹配
            allMembers = allMembers.Where(m =>
                m.NickName.Contains(kw) ||
                phoneMemberIds.Contains(m.Id));
        }

        var totalCount = allMembers.Count();
        var members = allMembers
            .OrderByDescending(m => m.CreatedAt)
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<MemberDto>>(members);

        var userIds = members.Select(m => m.UserId).Distinct().ToList();
        var users = _unitOfWork.Users.Find(u => userIds.Contains(u.Id)).ToList();

        foreach (var dto in dtos)
        {
            var user = users.FirstOrDefault(u => u.Id == dto.UserId);
            if (user != null)
            {
                dto.Phone = user.Phone;
                dto.Email = user.Email;
            }

            dto.LevelName = dto.Level switch
            {
                MemberLevel.Bronze => "青铜会员",
                MemberLevel.Silver => "白银会员",
                MemberLevel.Gold => "黄金会员",
                MemberLevel.Diamond => "钻石会员",
                _ => "未知"
            };
        }

        return Task.FromResult(PagedResult<MemberDto>.Create(dtos, queryParams.PageIndex, queryParams.PageSize, totalCount));
    }

    /// <inheritdoc />
    public Task<MemberDto> GetById(Guid id)
    {
        var member = _unitOfWork.Members.GetById(id);
        if (member == null)
        {
            throw new NotFoundException("会员不存在");
        }

        var dto = _mapper.Map<MemberDto>(member);

        // 关联用户表填充手机号/邮箱（列表查询同样逻辑）
        if (member.UserId != Guid.Empty)
        {
            var user = _unitOfWork.Users.GetById(member.UserId);
            if (user != null)
            {
                dto.Phone = user.Phone;
                dto.Email = user.Email;
            }
        }

        dto.LevelName = dto.Level switch
        {
            MemberLevel.Bronze => "青铜会员",
            MemberLevel.Silver => "白银会员",
            MemberLevel.Gold => "黄金会员",
            MemberLevel.Diamond => "钻石会员",
            _ => "未知"
        };
        return Task.FromResult(dto);
    }

    /// <inheritdoc />
    public Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var now = DateTime.Now;
        var passwordHash = PasswordHelper.HashPassword(dto.Password ?? "123456");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = dto.NickName,
            PasswordHash = passwordHash,
            Phone = dto.Phone,
            Email = dto.Email,
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
            NickName = dto.NickName,
            Level = dto.Level ?? MemberLevel.Bronze,
            Points = 0,
            Balance = 0,
            Birthday = dto.Birthday,
            Avatar = dto.Avatar,
            CreatedAt = now,
            UpdatedAt = now
        };
        _unitOfWork.Members.Add(member);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<MemberDto>(member);
        resultDto.Phone = user.Phone;
        resultDto.Email = user.Email;
        resultDto.LevelName = resultDto.Level switch
        {
            MemberLevel.Bronze => "青铜会员",
            MemberLevel.Silver => "白银会员",
            MemberLevel.Gold => "黄金会员",
            MemberLevel.Diamond => "钻石会员",
            _ => "未知"
        };
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Guid id, UpdateProfileDto dto)
    {
        return UpdateProfile(id, dto);
    }

    /// <inheritdoc />
    public Task UpdateMember(Guid id, UpdateMemberDto dto)
    {
        var member = _unitOfWork.Members.GetById(id);
        if (member == null)
        {
            throw new NotFoundException("会员不存在");
        }

        member.NickName = dto.NickName;
        member.Birthday = dto.Birthday;
        member.Avatar = dto.Avatar;
        if (dto.Level.HasValue)
        {
            member.Level = dto.Level.Value;
        }
        member.UpdatedAt = DateTime.Now;

        var user = _unitOfWork.Users.GetById(member.UserId);
        if (user != null)
        {
            user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                user.Phone = dto.Phone;
            }
            _unitOfWork.Users.Update(user);
        }

        _unitOfWork.Members.Update(member);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateProfile(Guid id, UpdateProfileDto dto)
    {
        // TODO: 实现更新会员资料
        // 1. 查找会员
        // 2. 更新NickName、Birthday、Avatar
        // 3. 同步更新用户表的Email
        // 4. 保存更改

        var member = _unitOfWork.Members.GetById(id);
        if (member == null)
        {
            throw new NotFoundException("会员不存在");
        }

        member.NickName = dto.NickName;
        member.Birthday = dto.Birthday;
        member.Avatar = dto.Avatar;
        member.UpdatedAt = DateTime.Now;

        var user = _unitOfWork.Users.GetById(member.UserId);
        if (user != null)
        {
            user.Email = dto.Email;
            _unitOfWork.Users.Update(user);
        }

        _unitOfWork.Members.Update(member);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<RechargeRecordDto> Recharge(RechargeDto dto)
    {
        // TODO: 实现会员充值
        // 1. 查找会员
        // 2. 计算赠送金额（根据充值阶梯规则）
        // 3. 更新会员余额
        // 4. 创建充值记录
        // 5. 保存更改

        var member = _unitOfWork.Members.GetById(dto.MemberId);
        if (member == null)
        {
            throw new NotFoundException("会员不存在");
        }

        var bonus = dto.Bonus ?? CalculateBonus(dto.Amount);
        var now = DateTime.Now;

        member.Balance += dto.Amount + bonus;
        member.UpdatedAt = now;

        var record = new RechargeRecord
        {
            Id = Guid.NewGuid(),
            MemberId = dto.MemberId,
            Amount = dto.Amount,
            Bonus = bonus,
            PayMethod = dto.PayMethod,
            CreatedAt = now
        };

        _unitOfWork.Members.Update(member);
        _unitOfWork.RechargeRecords.Add(record);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<RechargeRecordDto>(record);
        resultDto.NickName = member.NickName;
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task<List<RechargeRecordDto>> GetRechargeRecords(Guid memberId)
    {
        var member = _unitOfWork.Members.GetById(memberId);
        if (member == null)
        {
            throw new NotFoundException("会员不存在");
        }

        var records = _unitOfWork.RechargeRecords
            .Find(r => r.MemberId == memberId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var dtos = _mapper.Map<List<RechargeRecordDto>>(records);
        foreach (var dto in dtos)
        {
            dto.NickName = member.NickName;
        }

        return Task.FromResult(dtos);
    }

    /// <inheritdoc />
    public Task<List<OrderDto>> GetConsumptionHistory(Guid memberId)
    {
        // TODO: 实现获取消费历史
        // 1. 查找会员的所有订单
        // 2. 按时间倒序排序
        // 3. 映射为DTO

        var orders = _unitOfWork.Orders
            .Find(o => o.MemberId == memberId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        var dtos = _mapper.Map<List<OrderDto>>(orders);
        return Task.FromResult(dtos);
    }

    /// <inheritdoc />
    public Task<List<MemberBalanceRecordDto>> GetBalanceRecords(Guid memberId)
    {
        var records = _unitOfWork.MemberBalanceRecords
            .Find(r => r.MemberId == memberId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var dtos = records.Select(r =>
        {
            var dto = new MemberBalanceRecordDto
            {
                Id = r.Id,
                TypeName = r.Type == "Consume" ? "消费扣款" : "余额变动",
                Amount = r.Amount,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            };

            if (r.OrderId.HasValue)
            {
                var order = _unitOfWork.Orders.GetById(r.OrderId.GetValueOrDefault());
                dto.OrderNo = order?.OrderNo;
            }

            return dto;
        }).ToList();

        return Task.FromResult(dtos);
    }

    /// <summary>
    /// 计算充值赠送金额
    /// </summary>
    /// <param name="amount">充值金额</param>
    /// <returns>赠送金额</returns>
    private static decimal CalculateBonus(decimal amount)
    {
        // TODO: 根据实际充值阶梯规则调整
        return amount switch
        {
            >= 1000 => amount * 0.2m,
            >= 500 => amount * 0.1m,
            >= 200 => amount * 0.05m,
            _ => 0
        };
    }
}
