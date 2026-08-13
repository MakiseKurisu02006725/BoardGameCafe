using AutoMapper;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 操作日志服务实现
/// </summary>
public class OperationLogService : IOperationLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OperationLogService> _logger;

    /// <summary>
    /// 初始化操作日志服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public OperationLogService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OperationLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PagedResult<OperationLogDto>> GetPagedList(LogQueryParams queryParams)
    {
        var query = _unitOfWork.OperationLogs.GetAll().AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(l =>
                (l.Description != null && l.Description.Contains(queryParams.Keyword)) ||
                (l.TargetId != null && l.TargetId.Contains(queryParams.Keyword)));
        }

        if (queryParams.UserId.HasValue)
        {
            query = query.Where(l => l.UserId == queryParams.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Action))
        {
            query = query.Where(l => l.Action == queryParams.Action);
        }

        if (queryParams.DateFrom.HasValue)
        {
            query = query.Where(l => l.Timestamp >= queryParams.DateFrom.Value);
        }

        if (queryParams.DateTo.HasValue)
        {
            query = query.Where(l => l.Timestamp <= queryParams.DateTo.Value);
        }

        var totalCount = query.Count();
        var logs = query
            .OrderByDescending(l => l.Timestamp)
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<OperationLogDto>>(logs);
        return Task.FromResult(PagedResult<OperationLogDto>.Create(dtos, queryParams.PageIndex, queryParams.PageSize, totalCount));
    }

    /// <inheritdoc />
    public Task LogAction(Guid? userId, string userName, string action, string? targetId, string? description)
    {
        var log = new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName ?? "系统",
            Action = action ?? string.Empty,
            TargetId = targetId,
            Description = description,
            Timestamp = DateTime.Now
        };

        _unitOfWork.OperationLogs.Add(log);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }
}
