using AutoMapper;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;
using BoardGameCafe.Common.Exceptions;
using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 用户员工服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// 初始化用户服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PagedResult<UserDto>> GetPagedList(UserQueryParams queryParams)
    {
        var allUsers = _unitOfWork.Users
            .Find(u => !u.IsDeleted && u.Role != Core.Enums.UserRole.Member)
            .AsQueryable();

        if (queryParams.Role.HasValue)
        {
            allUsers = allUsers.Where(u => u.Role == queryParams.Role.Value);
        }

        if (!string.IsNullOrEmpty(queryParams.Keyword))
        {
            var kw = queryParams.Keyword.Trim();
            allUsers = allUsers.Where(u =>
                u.UserName.Contains(kw) ||
                (u.Phone != null && u.Phone.Contains(kw)));
        }

        allUsers = allUsers.OrderByDescending(u => u.CreatedAt);

        var totalCount = allUsers.Count();
        var users = allUsers
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<UserDto>>(users);
        return Task.FromResult(PagedResult<UserDto>.Create(dtos, queryParams.PageIndex, queryParams.PageSize, totalCount));
    }

    /// <inheritdoc />
    public Task<UserDto> GetById(Guid id)
    {
        var user = _unitOfWork.Users.GetById(id);
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("用户不存在");
        }

        var dto = _mapper.Map<UserDto>(user);
        return Task.FromResult(dto);
    }

    /// <inheritdoc />
    public Task<UserDto> Create(CreateUserDto dto)
    {
        // TODO: 检查用户名和手机号是否重复
        var existingByPhone = _unitOfWork.Users
            .Find(u => u.Phone == dto.Phone && !u.IsDeleted)
            .FirstOrDefault();
        if (existingByPhone != null)
        {
            throw new BusinessException("该手机号已注册");
        }

        var existingByUserName = _unitOfWork.Users
            .Find(u => u.UserName == dto.UserName && !u.IsDeleted)
            .FirstOrDefault();
        if (existingByUserName != null)
        {
            throw new BusinessException("用户名已存在");
        }

        if (!PasswordHelper.ValidatePasswordStrength(dto.Password))
        {
            throw new BusinessException("密码强度不足，至少6位");
        }

        var now = DateTime.Now;
        var user = _mapper.Map<User>(dto);
        user.Id = Guid.NewGuid();
        user.PasswordHash = PasswordHelper.HashPassword(dto.Password);
        user.CreatedAt = now;
        user.IsDeleted = false;

        _unitOfWork.Users.Add(user);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<UserDto>(user);
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task Update(Guid id, UpdateUserDto dto)
    {
        var user = _unitOfWork.Users.GetById(id);
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("用户不存在");
        }

        if (dto.Phone != null)
            user.Phone = dto.Phone;
        if (dto.Email != null)
            user.Email = dto.Email;
        if (dto.Role.HasValue)
            user.Role = dto.Role.Value;
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = PasswordHelper.HashPassword(dto.Password);
        }

        _unitOfWork.Users.Update(user);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Delete(Guid id)
    {
        var user = _unitOfWork.Users.GetById(id);
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("用户不存在");
        }

        user.IsDeleted = true;
        _unitOfWork.Users.Update(user);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ChangePassword(Guid id, ChangePasswordDto dto)
    {
        var user = _unitOfWork.Users.GetById(id);
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("用户不存在");
        }

        if (!PasswordHelper.VerifyPassword(dto.OldPassword, user.PasswordHash))
        {
            throw new BusinessException("原密码错误");
        }

        if (!PasswordHelper.ValidatePasswordStrength(dto.NewPassword))
        {
            throw new BusinessException("新密码强度不足，至少6位");
        }

        user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
        _unitOfWork.Users.Update(user);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ResetPassword(Guid id, ResetPasswordDto dto)
    {
        var user = _unitOfWork.Users.GetById(id);
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("用户不存在");
        }

        if (!PasswordHelper.ValidatePasswordStrength(dto.NewPassword))
        {
            throw new BusinessException("新密码强度不足，至少6位");
        }

        user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
        _unitOfWork.Users.Update(user);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }
}
