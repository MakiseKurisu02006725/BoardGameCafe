using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common.Exceptions;
using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// 初始化认证服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="jwtSettings">JWT配置选项</param>
    /// <param name="logger">日志记录器</param>
    public AuthService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<LoginResponseDto> Login(LoginDto dto)
    {
        // 安全校验：空密码直接拒绝（避免 PasswordHelper 对空字符串抛异常返回 500）
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new BusinessException("手机号或密码错误");
        }

        // 暴力破解防护：同一手机号连续失败锁定（锁定期间直接拒绝）
        if (_loginFailures.TryGetValue(dto.Phone, out var lockEntry) && lockEntry.LockUntil > DateTime.Now)
        {
            throw new BusinessException("尝试次数过多，请15分钟后再试");
        }

        var user = _unitOfWork.Users.Find(u => u.Phone == dto.Phone && !u.IsDeleted).FirstOrDefault();
        if (user == null)
        {
            RecordLoginFailure(dto.Phone);
            throw new BusinessException("手机号或密码错误");
        }

        if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
        {
            RecordLoginFailure(dto.Phone);
            throw new BusinessException("手机号或密码错误");
        }

        // 登录成功：清除该手机号的失败记录
        _loginFailures.TryRemove(dto.Phone, out _);

        var member = _unitOfWork.Members.Find(m => m.UserId == user.Id).FirstOrDefault();
        var nickName = member?.NickName ?? user.UserName;

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        _refreshTokenStore.TryAdd(refreshToken, (user.Id, DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays)));

        return Task.FromResult(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            UserId = user.Id,
            Role = user.Role.ToString(),
            NickName = nickName
        });
    }

    // 登录失败计数：手机号 -> (连续失败次数, 锁定截止时间)
    private sealed class LoginFailure
    {
        public int Count { get; set; }
        public DateTime LockUntil { get; set; }
    }

    private static readonly ConcurrentDictionary<string, LoginFailure> _loginFailures = new();

    /// <summary>
    /// 记录一次登录失败，连续失败达到阈值后锁定
    /// </summary>
    private static void RecordLoginFailure(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }

        if (_loginFailures.TryGetValue(phone, out var entry))
        {
            entry.Count++;
            if (entry.Count >= 5)
            {
                entry.LockUntil = DateTime.Now.AddMinutes(15);
                entry.Count = 0;
            }
        }
        else
        {
            _loginFailures.TryAdd(phone, new LoginFailure { Count = 1 });
        }
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> Register(RegisterDto dto)
    {
        // TODO: 实现用户注册逻辑
        // 1. 检查手机号是否已存在
        // 2. 创建User（Role=Member）和Member
        // 3. 保存更改
        // 4. 返回Login结果

        var existingUser = _unitOfWork.Users.Find(u => u.Phone == dto.Phone && !u.IsDeleted).FirstOrDefault();
        if (existingUser != null)
        {
            throw new BusinessException("该手机号已注册");
        }

        var userId = Guid.NewGuid();
        var now = DateTime.Now;

        var user = new User
        {
            Id = userId,
            UserName = dto.Phone,
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            Role = UserRole.Member,
            Phone = dto.Phone,
            Email = dto.Email,
            CreatedAt = now,
            IsDeleted = false
        };

        var member = new Member
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NickName = dto.NickName,
            Level = MemberLevel.Bronze,
            Points = 0,
            Balance = 0,
            Birthday = null,
            Avatar = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _unitOfWork.Users.Add(user);
        _unitOfWork.Members.Add(member);
        _unitOfWork.SaveChanges();

        return await Login(new LoginDto { Phone = dto.Phone, Password = dto.Password });
    }

    // 内存存储 refreshToken -> userId 映射（生产环境应使用数据库或缓存）
    private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)> _refreshTokenStore = new();

    /// <inheritdoc />
    public Task<LoginResponseDto> RefreshToken(RefreshTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            throw new BusinessException("RefreshToken 不能为空");
        }

        if (!_refreshTokenStore.TryGetValue(dto.RefreshToken, out var entry))
        {
            throw new BusinessException("登录凭证已过期，请重新登录");
        }

        if (entry.ExpiresAt < DateTime.Now)
        {
            _refreshTokenStore.TryRemove(dto.RefreshToken, out _);
            throw new BusinessException("登录凭证已过期，请重新登录");
        }

        var user = _unitOfWork.Users.Find(u => u.Id == entry.UserId && !u.IsDeleted).FirstOrDefault();
        if (user == null)
        {
            throw new BusinessException("用户不存在");
        }

        var member = _unitOfWork.Members.Find(m => m.UserId == user.Id).FirstOrDefault();
        var nickName = member?.NickName ?? user.UserName;

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        _refreshTokenStore.TryRemove(dto.RefreshToken, out _);
        _refreshTokenStore.TryAdd(newRefreshToken, (user.Id, DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays)));

        return Task.FromResult(new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            UserId = user.Id,
            Role = user.Role.ToString(),
            NickName = nickName
        });
    }

    /// <inheritdoc />
    public Task Logout(Guid userId)
    {
        // 登出时移除该用户的所有刷新令牌，使其立即失效（防登出后令牌仍可用）
        var tokens = _refreshTokenStore
            .Where(kv => kv.Value.UserId == userId)
            .Select(kv => kv.Key)
            .ToList();
        foreach (var token in tokens)
        {
            _refreshTokenStore.TryRemove(token, out _);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<UserInfoDto> GetUserInfo(Guid userId)
    {
        var user = _unitOfWork.Users.Find(u => u.Id == userId && !u.IsDeleted).FirstOrDefault();
        if (user == null)
        {
            throw new BusinessException("用户不存在");
        }

        var member = _unitOfWork.Members.Find(m => m.UserId == userId).FirstOrDefault();

        return Task.FromResult(new UserInfoDto
        {
            UserId = user.Id,
            MemberId = member?.Id,
            UserName = user.UserName,
            NickName = member?.NickName ?? user.UserName,
            Role = user.Role.ToString(),
            Avatar = member?.Avatar,
            Phone = user.Phone
        });
    }

    /// <summary>
    /// 生成JWT访问令牌
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>JWT令牌字符串</returns>
    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <returns>刷新令牌字符串</returns>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
