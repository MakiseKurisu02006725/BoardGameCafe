using AutoMapper;
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
/// 桌游服务实现
/// </summary>
public class BoardGameService : IBoardGameService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BoardGameService> _logger;

    /// <summary>
    /// 初始化桌游服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public BoardGameService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<BoardGameService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PagedResult<BoardGameDto>> GetPagedList(GameQueryParams queryParams)
    {
        // TODO: 实现桌游分页查询
        // 1. 过滤已删除的桌游
        // 2. 应用Keyword、Category、玩家数、库存等筛选条件
        // 3. 应用排序和分页

        var query = _unitOfWork.BoardGames.GetAll().AsQueryable().Where(g => !g.IsDeleted);

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(g => g.Name.Contains(queryParams.Keyword));
        }

        if (queryParams.Category.HasValue)
        {
            query = query.Where(g => g.Category == queryParams.Category.Value);
        }

        if (queryParams.MinPlayers.HasValue)
        {
            query = query.Where(g => g.MinPlayers <= queryParams.MinPlayers.Value);
        }

        if (queryParams.MaxPlayers.HasValue)
        {
            query = query.Where(g => g.MaxPlayers >= queryParams.MaxPlayers.Value);
        }

        if (queryParams.InStockOnly == true)
        {
            query = query.Where(g => g.CurrentStock > 0);
        }

        var totalCount = query.Count();
        var games = query
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<BoardGameDto>>(games);
        return Task.FromResult(PagedResult<BoardGameDto>.Create(dtos, queryParams.PageIndex, queryParams.PageSize, totalCount));
    }

    /// <inheritdoc />
    public Task<BoardGameDto> GetById(Guid id)
    {
        var game = _unitOfWork.BoardGames.GetById(id);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        var dto = _mapper.Map<BoardGameDto>(game);
        return Task.FromResult(dto);
    }

    /// <inheritdoc />
    public Task<BoardGameDto> Create(CreateBoardGameDto dto)
    {
        var now = DateTime.Now;
        var game = _mapper.Map<BoardGame>(dto);
        game.Id = Guid.NewGuid();
        game.GameNo = BusinessNoGenerator.NextGameNo(
            _unitOfWork.BoardGames.GetAll().Where(g => !g.IsDeleted).Select(g => g.GameNo));
        game.CreatedAt = now;
        game.UpdatedAt = now;

        _unitOfWork.BoardGames.Add(game);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<BoardGameDto>(game);
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task Update(Guid id, UpdateBoardGameDto dto)
    {
        var game = _unitOfWork.BoardGames.GetById(id);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
            game.Name = dto.Name;
        if (dto.Category.HasValue)
            game.Category = dto.Category.Value;
        if (dto.MinPlayers.HasValue)
            game.MinPlayers = dto.MinPlayers.Value;
        if (dto.MaxPlayers.HasValue)
            game.MaxPlayers = dto.MaxPlayers.Value;
        if (dto.Description != null)
            game.Description = dto.Description;
        if (dto.ImageUrl != null)
            game.ImageUrl = dto.ImageUrl;
        if (dto.TotalStock.HasValue)
        {
            var diff = dto.TotalStock.Value - game.TotalStock;
            game.TotalStock = dto.TotalStock.Value;
            game.CurrentStock += diff;
        }
        if (dto.DamagedStock.HasValue)
            game.DamagedStock = dto.DamagedStock.Value;
        if (dto.PricePerHour.HasValue)
            game.PricePerHour = dto.PricePerHour.Value;

        game.UpdatedAt = DateTime.Now;
        _unitOfWork.BoardGames.Update(game);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Delete(Guid id)
    {
        var game = _unitOfWork.BoardGames.GetById(id);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        game.IsDeleted = true;
        game.UpdatedAt = DateTime.Now;
        _unitOfWork.BoardGames.Update(game);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<string>> Borrow(Guid id, int quantity)
    {
        var game = _unitOfWork.BoardGames.GetById(id);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        if (game.CurrentStock < quantity)
        {
            throw new BusinessException("库存不足");
        }

        var now = DateTime.Now;
        var prefix = PinyinHelper.GetInitials(game.Name);
        var usedNumbers = _unitOfWork.BoardGameCopies
            .Find(c => c.BoardGameId == id)
            .Select(c => c.CopyNumber)
            .ToList();
        var nextSeq = ParseCopyMaxSeq(usedNumbers, prefix) + 1;

        // 优先复用已在库的副本，不足时新建副本并分配编号
        var inStockCopies = _unitOfWork.BoardGameCopies
            .Find(c => c.BoardGameId == id && c.Status == CopyStatus.InStock)
            .OrderBy(c => c.CopyNumber)
            .ToList();

        var assignedNos = new List<string>();
        for (var i = 0; i < quantity; i++)
        {
            BoardGameCopy copy;
            if (i < inStockCopies.Count)
            {
                copy = inStockCopies[i];
                copy.Status = CopyStatus.Borrowed;
                _unitOfWork.BoardGameCopies.Update(copy);
            }
            else
            {
                var copyNo = $"{prefix}{nextSeq++:D3}";
                copy = new BoardGameCopy
                {
                    Id = Guid.NewGuid(),
                    BoardGameId = id,
                    CopyNumber = copyNo,
                    Status = CopyStatus.Borrowed,
                    CreatedAt = now
                };
                _unitOfWork.BoardGameCopies.Add(copy);
            }

            assignedNos.Add(copy.CopyNumber);
        }

        game.CurrentStock -= quantity;
        game.UpdatedAt = now;
        _unitOfWork.BoardGames.Update(game);
        _unitOfWork.SaveChanges();

        return Task.FromResult(assignedNos);
    }

    /// <inheritdoc />
    public Task Return(Guid id, int quantity)
    {
        var game = _unitOfWork.BoardGames.GetById(id);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        var maxReturn = game.TotalStock - game.DamagedStock;
        if (game.CurrentStock + quantity > maxReturn)
        {
            throw new BusinessException("归还数量异常");
        }

        // 将对应数量的"借出"副本标记为在库
        var borrowedCopies = _unitOfWork.BoardGameCopies
            .Find(c => c.BoardGameId == id && c.Status == CopyStatus.Borrowed)
            .OrderBy(c => c.CopyNumber)
            .Take(quantity)
            .ToList();

        foreach (var copy in borrowedCopies)
        {
            copy.Status = CopyStatus.InStock;
            _unitOfWork.BoardGameCopies.Update(copy);
        }

        game.CurrentStock += quantity;
        game.UpdatedAt = DateTime.Now;
        _unitOfWork.BoardGames.Update(game);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Restock(Guid id, RestockDto dto)
    {
        var game = _unitOfWork.BoardGames.GetById(id);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        if (dto.Quantity <= 0)
        {
            throw new BusinessException("补货数量必须大于0");
        }

        var now = DateTime.Now;
        var prefix = PinyinHelper.GetInitials(game.Name);
        var usedNumbers = _unitOfWork.BoardGameCopies
            .Find(c => c.BoardGameId == id)
            .Select(c => c.CopyNumber)
            .ToList();
        var nextSeq = ParseCopyMaxSeq(usedNumbers, prefix) + 1;

        // 优先修复损坏副本（复用已损坏编号），不足时新建副本
        var damagedCopies = _unitOfWork.BoardGameCopies
            .Find(c => c.BoardGameId == id && c.Status == CopyStatus.Damaged)
            .OrderBy(c => c.CopyNumber)
            .ToList();

        var reused = Math.Min(dto.Quantity, damagedCopies.Count);

        // 事务包裹：副本新增/损坏修复、库存更新、补货记录写入保证原子性，数据库异常时整体回滚（避免部分写入导致"提示成功但库存未变"）
        _unitOfWork.BeginTransaction();
        try
        {
            for (var i = 0; i < reused; i++)
            {
                damagedCopies[i].Status = CopyStatus.InStock;
                _unitOfWork.BoardGameCopies.Update(damagedCopies[i]);
            }

            for (var i = reused; i < dto.Quantity; i++)
            {
                var copyNo = $"{prefix}{nextSeq++:D3}";
                _unitOfWork.BoardGameCopies.Add(new BoardGameCopy
                {
                    Id = Guid.NewGuid(),
                    BoardGameId = id,
                    CopyNumber = copyNo,
                    Status = CopyStatus.InStock,
                    CreatedAt = now
                });
            }

            // 总库存只对真正新增的副本累加（复用的损坏副本编号已存在，只改变状态）
            game.TotalStock += dto.Quantity - reused;
            game.CurrentStock += dto.Quantity;
            game.DamagedStock = Math.Max(0, game.DamagedStock - reused);
            game.UpdatedAt = now;

            _unitOfWork.BoardGames.Update(game);
            _unitOfWork.RestockRecords.Add(new RestockRecord
            {
                Id = Guid.NewGuid(),
                BoardGameId = id,
                Quantity = dto.Quantity,
                Remark = dto.Remark,
                CreatedAt = now
            });

            _unitOfWork.Commit();
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<DamageRecordDto> RecordDamage(CreateDamageDto dto, Guid createdBy)
    {
        // TODO: 实现损坏记录
        // 1. 查找桌游
        // 2. 增加损坏库存，减少当前库存
        // 3. 创建损坏记录
        // 4. 保存更改

        var game = _unitOfWork.BoardGames.GetById(dto.BoardGameId);
        if (game == null || game.IsDeleted)
        {
            throw new NotFoundException("桌游不存在");
        }

        var now = DateTime.Now;
        game.DamagedStock += dto.Quantity;
        game.CurrentStock = Math.Max(0, game.CurrentStock - dto.Quantity);
        game.UpdatedAt = now;

        // 将对应数量的"在库"副本标记为损坏
        var inStockCopies = _unitOfWork.BoardGameCopies
            .Find(c => c.BoardGameId == dto.BoardGameId && c.Status == CopyStatus.InStock)
            .OrderBy(c => c.CopyNumber)
            .Take(dto.Quantity)
            .ToList();
        foreach (var copy in inStockCopies)
        {
            copy.Status = CopyStatus.Damaged;
            _unitOfWork.BoardGameCopies.Update(copy);
        }

        var record = new DamageRecord
        {
            Id = Guid.NewGuid(),
            BoardGameId = dto.BoardGameId,
            OrderId = dto.OrderId,
            Quantity = dto.Quantity,
            Reason = dto.Reason,
            Compensation = dto.Compensation ?? 0,
            CreatedBy = createdBy,
            CreatedAt = now
        };

        var user = _unitOfWork.Users.GetById(createdBy);

        _unitOfWork.BoardGames.Update(game);
        _unitOfWork.DamageRecords.Add(record);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<DamageRecordDto>(record);
        resultDto.BoardGameName = game.Name;
        resultDto.CreatedByName = user?.UserName ?? "未知";
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task<List<BorrowedGameDto>> GetBorrowed()
    {
        // 开台借出（订单-桌游关联表，未归还）
        var borrowedItems = _unitOfWork.OrderBoardGames
            .Find(x => !x.IsReturned)
            .ToList();
        // 预约借出（桌游副本表，Borrowed 且关联预约）
        var reservationCopies = _unitOfWork.BoardGameCopies
            .Find(c => c.Status == CopyStatus.Borrowed && c.ReservationId.HasValue)
            .ToList();

        if (borrowedItems.Count == 0 && reservationCopies.Count == 0)
        {
            return Task.FromResult(new List<BorrowedGameDto>());
        }

        var games = _unitOfWork.BoardGames.GetAll().Where(g => !g.IsDeleted).ToList();
        var gameMap = games.ToDictionary(g => g.Id, g => g);

        var result = new List<BorrowedGameDto>();

        // 开台借出：按订单聚合桌游数量，关联订单号/桌位号
        var orderIds = borrowedItems.Select(x => x.OrderId).Distinct().ToList();
        var orders = _unitOfWork.Orders.Find(o => orderIds.Contains(o.Id)).ToList();
        var orderMap = orders.ToDictionary(o => o.Id, o => o);

        foreach (var group in borrowedItems.GroupBy(x => x.BoardGameId))
        {
            var game = gameMap.TryGetValue(group.Key, out var g) ? g : null;
            var first = group.First();
            var order = orderMap.TryGetValue(first.OrderId, out var o) ? o : null;

            result.Add(new BorrowedGameDto
            {
                BoardGameId = group.Key,
                Name = game?.Name ?? "未知桌游",
                GameNo = game?.GameNo ?? string.Empty,
                Quantity = group.Sum(x => x.Quantity),
                OrderId = first.OrderId,
                OrderNo = order?.OrderNo ?? string.Empty,
                TableCode = order?.TableId.HasValue == true
                    ? _unitOfWork.Tables.GetById(order.TableId.Value)?.Code
                    : null,
                OrderStatus = order?.Status ?? OrderStatus.Pending
            });
        }

        // 预约借出：副本关联预约，按桌游聚合，关联预约号
        var reservationIds = reservationCopies.Select(c => c.ReservationId ?? Guid.Empty).Distinct().ToList();
        var reservations = _unitOfWork.Reservations.Find(r => reservationIds.Contains(r.Id)).ToList();
        var resvMap = reservations.ToDictionary(r => r.Id, r => r);

        foreach (var group in reservationCopies.GroupBy(c => c.BoardGameId))
        {
            var game = gameMap.TryGetValue(group.Key, out var g) ? g : null;
            var copy = group.First();
            var resv = resvMap.TryGetValue(copy.ReservationId ?? Guid.Empty, out var r) ? r : null;

            result.Add(new BorrowedGameDto
            {
                BoardGameId = group.Key,
                Name = game?.Name ?? "未知桌游",
                GameNo = game?.GameNo ?? string.Empty,
                Quantity = group.Count(),
                OrderId = Guid.Empty,
                OrderNo = resv?.ReservationNo ?? string.Empty,
                TableCode = resv?.TableId.HasValue == true
                    ? _unitOfWork.Tables.GetById(resv.TableId.Value)?.Code
                    : null,
                OrderStatus = OrderStatus.Pending
            });
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// 解析指定前缀副本编号中的最大数字序号（如 CG001 → 1）
    /// </summary>
    /// <param name="copyNumbers">副本编号集合</param>
    /// <param name="prefix">编号前缀</param>
    /// <returns>最大序号（无匹配时为0）</returns>
    private static int ParseCopyMaxSeq(IEnumerable<string> copyNumbers, string prefix)
    {
        return copyNumbers
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && n.Length > prefix.Length)
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
    }
}
