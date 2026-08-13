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
/// 预约服务实现
/// </summary>
public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationService> _logger;

    /// <summary>
    /// 初始化预约服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public ReservationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReservationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PagedResult<ReservationDto>> GetPagedList(ReservationQueryParams queryParams)
    {
        // TODO: 实现预约分页查询
        // 1. 应用Status、会员Id、日期范围筛选
        // 2. 关联会员表、桌位表获取名称
        // 3. 应用排序和分页

        var query = _unitOfWork.Reservations.GetAll().AsQueryable();

        if (queryParams.Status.HasValue)
        {
            query = query.Where(r => r.Status == queryParams.Status.Value);
        }

        if (queryParams.MemberId.HasValue)
        {
            query = query.Where(r => r.MemberId == queryParams.MemberId.Value);
        }

        if (queryParams.DateFrom.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= queryParams.DateFrom.Value);
        }

        if (queryParams.DateTo.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= queryParams.DateTo.Value);
        }

        var totalCount = query.Count();
        var reservations = query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ReservationDto>>(reservations);
        foreach (var dto in dtos)
        {
            var member = _unitOfWork.Members.GetById(dto.MemberId);
            dto.MemberName = member?.NickName ?? "未知";
            if (dto.TableId.HasValue)
            {
                var table = _unitOfWork.Tables.GetById(dto.TableId.Value);
                dto.TableCode = table?.Code;
            }

            dto.TableTypeName = dto.TableType switch
            {
                TableArea.Hall => "大厅",
                TableArea.PrivateRoom => "包间",
                TableArea.Booth => "卡座",
                _ => "未知"
            };

            dto.StatusName = dto.Status switch
            {
                ReservationStatus.Pending => "待确认",
                ReservationStatus.Confirmed => "已确认",
                ReservationStatus.Completed => "已完成",
                ReservationStatus.Cancelled => "已取消",
                ReservationStatus.Expired => "已过期",
                _ => "未知"
            };
        }

        return Task.FromResult(PagedResult<ReservationDto>.Create(dtos, queryParams.PageIndex, queryParams.PageSize, totalCount));
    }

    /// <inheritdoc />
    public Task<ReservationDto> Create(CreateReservationDto dto, Guid memberId)
    {
        // TODO: 实现创建预约逻辑
        // 1. 冲突检测：检查同区域、同时段是否有已确认的预约
        // 2. 计算押金金额
        // 3. 创建预约记录
        // 4. 保存更改

        var member = _unitOfWork.Members.GetById(memberId);
        if (member == null)
        {
            throw new NotFoundException("会员不存在");
        }

        // 冲突检测
        var hasConflict = _unitOfWork.Reservations
            .Find(r =>
                r.TableType == dto.TableType &&
                r.Date == dto.Date &&
                r.Status == ReservationStatus.Confirmed &&
                !(dto.EndTime <= r.StartTime || dto.StartTime >= r.EndTime))
            .Any();

        if (hasConflict)
        {
            throw new BusinessException("该时段已有预约，请选择其他时间");
        }

        // 指定桌位时校验：维修状态的桌位不可预约
        if (dto.TableId.HasValue)
        {
            var targetTable = _unitOfWork.Tables.GetById(dto.TableId.Value);
            if (targetTable != null && targetTable.Status == TableStatus.Maintenance)
            {
                throw new BusinessException("维修状态的桌位不能预约，请选择其他桌位");
            }
        }

        var now = DateTime.Now;
        var reservation = _mapper.Map<Reservation>(dto);
        reservation.Id = Guid.NewGuid();
        reservation.ReservationNo = BusinessNoGenerator.NextReservationNo(
            _unitOfWork.Reservations.GetAll().Select(r => r.ReservationNo));
        reservation.MemberId = memberId;
        reservation.TableId = dto.TableId;
        reservation.ReservedGames = ResolveGameNames(dto.GameIds);
        reservation.ReservedGameIds = dto.GameIds != null && dto.GameIds.Count > 0
            ? string.Join(",", dto.GameIds)
            : null;
        reservation.Deposit = CalculateDeposit(dto.TableType, dto.StartTime, dto.EndTime);
        reservation.CreatedAt = now;
        reservation.UpdatedAt = now;

        _unitOfWork.Reservations.Add(reservation);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<ReservationDto>(reservation);
        resultDto.MemberName = member.NickName;
        if (resultDto.TableId.HasValue)
        {
            var table = _unitOfWork.Tables.GetById(resultDto.TableId.Value);
            resultDto.TableCode = table?.Code;
        }

        resultDto.TableTypeName = resultDto.TableType switch
        {
            TableArea.Hall => "大厅",
            TableArea.PrivateRoom => "包间",
            TableArea.Booth => "卡座",
            _ => "未知"
        };
        resultDto.StatusName = resultDto.Status switch
        {
            ReservationStatus.Pending => "待确认",
            ReservationStatus.Confirmed => "已确认",
            ReservationStatus.Completed => "已完成",
            ReservationStatus.Cancelled => "已取消",
            ReservationStatus.Expired => "已过期",
            _ => "未知"
        };
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task Confirm(Guid id)
    {
        var reservation = _unitOfWork.Reservations.GetById(id);
        if (reservation == null)
        {
            throw new NotFoundException("预约不存在");
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new BusinessException("只有待确认的预约才能确认");
        }

        reservation.Status = ReservationStatus.Confirmed;
        reservation.UpdatedAt = DateTime.Now;

        // 预约确认后锁定对应桌位
        LockTable(reservation.TableId, TableStatus.Reserved);

        // 预约确认后自动借出所选桌游（库存 -1，副本标记为预约借出）
        BorrowReservationGames(id);

        _unitOfWork.Reservations.Update(reservation);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Cancel(Guid id)
    {
        var reservation = _unitOfWork.Reservations.GetById(id);
        if (reservation == null)
        {
            throw new NotFoundException("预约不存在");
        }

        if (reservation.Status == ReservationStatus.Completed ||
            reservation.Status == ReservationStatus.Cancelled ||
            reservation.Status == ReservationStatus.Expired)
        {
            throw new BusinessException("当前预约状态不允许取消");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.Now;

        // 取消预约后释放对应桌位
        LockTable(reservation.TableId, TableStatus.Idle);

        // 取消预约后自动归还已借出的桌游（库存 +1）
        ReturnReservationGames(id);

        _unitOfWork.Reservations.Update(reservation);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Complete(Guid id)
    {
        var reservation = _unitOfWork.Reservations.GetById(id);
        if (reservation == null)
        {
            throw new NotFoundException("预约不存在");
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException("只有已确认的预约才能完成");
        }

        reservation.Status = ReservationStatus.Completed;
        reservation.UpdatedAt = DateTime.Now;

        // 完成预约：释放桌位并自动归还预约桌游（库存 +1）
        LockTable(reservation.TableId, TableStatus.Idle);
        ReturnReservationGames(id);

        _unitOfWork.Reservations.Update(reservation);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OrderDto> ConvertToOrder(Guid id, Guid? tableId, Guid createdBy)
    {
        // TODO: 实现预约转订单逻辑
        // 1. 查找预约，检查状态是否为已确认
        // 2. 检查桌位状态是否空闲
        // 3. 更新预约状态为已完成
        // 4. 开台创建订单
        // 5. 保存更改

        var reservation = _unitOfWork.Reservations.GetById(id);
        if (reservation == null)
        {
            throw new NotFoundException("预约不存在");
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException("只有已确认的预约才能开台");
        }

        // 如果 tableId 为空，根据预约的 TableType 自动分配第一个空闲桌位
        if (!tableId.HasValue)
        {
            var autoTable = _unitOfWork.Tables
                .Find(t => t.Area == reservation.TableType && t.Status == TableStatus.Idle)
                .FirstOrDefault();

            if (autoTable == null)
            {
                throw new BusinessException("没有可用的桌位");
            }

            tableId = autoTable.Id;
        }

        var table = _unitOfWork.Tables.GetById(tableId.Value);
        if (table == null)
        {
            throw new NotFoundException("桌位不存在");
        }

        if (table.Status != TableStatus.Idle && table.Status != TableStatus.Reserved)
        {
            throw new BusinessException("桌位当前不可用");
        }

        var now = DateTime.Now;
        reservation.Status = ReservationStatus.Completed;
        reservation.UpdatedAt = now;

        table.Status = TableStatus.Occupied;
        table.UpdatedAt = now;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNo = GenerateOrderNo(),
            MemberId = reservation.MemberId,
            ReservationId = reservation.Id, // 关联预约，结账时自动归还预约桌游
            TableId = tableId.Value,
            StartTime = now,
            EndTime = null,
            TotalAmount = 0,
            Discount = 0,
            ActualAmount = 0,
            Status = OrderStatus.Pending,
            PaymentMethod = null,
            CreatedBy = createdBy,
            PaidAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _unitOfWork.Reservations.Update(reservation);
        _unitOfWork.Tables.Update(table);
        _unitOfWork.Orders.Add(order);
        _unitOfWork.SaveChanges();

        var orderDto = _mapper.Map<OrderDto>(order);
        orderDto.TableCode = table.Code;
        var member = _unitOfWork.Members.GetById(reservation.MemberId);
        orderDto.MemberName = member?.NickName;
        return Task.FromResult(orderDto);
    }

    /// <summary>
    /// 生成订单号
    /// </summary>
    /// <returns>订单号</returns>
    private string GenerateOrderNo()
    {
        var prefix = "ORD" + DateTime.Now.ToString("yyyyMMdd");
        var todayOrderNos = _unitOfWork.Orders
            .Find(o => o.OrderNo.StartsWith(prefix))
            .Select(o => o.OrderNo);
        return BusinessNoGenerator.NextOrderNo(todayOrderNos);
    }

    /// <summary>
    /// 预约确认时自动借出所选桌游：各借出一个在库副本（状态→借出中、记录来源与预约Id），桌游当前库存 -1
    /// </summary>
    /// <param name="reservationId">预约Id</param>
    private void BorrowReservationGames(Guid reservationId)
    {
        var reservation = _unitOfWork.Reservations.GetById(reservationId);
        if (reservation == null || string.IsNullOrWhiteSpace(reservation.ReservedGameIds))
        {
            return;
        }

        var gameIds = reservation.ReservedGameIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        var now = DateTime.Now;
        foreach (var gameId in gameIds)
        {
            var game = _unitOfWork.BoardGames.GetById(gameId);
            if (game == null || game.IsDeleted || game.CurrentStock <= 0)
            {
                continue; // 无在库库存则跳过，预约仍可确认
            }

            // 借出一个在库副本（优先最小编号）
            var copy = _unitOfWork.BoardGameCopies
                .Find(c => c.BoardGameId == gameId && c.Status == CopyStatus.InStock)
                .OrderBy(c => c.CopyNumber)
                .FirstOrDefault();
            if (copy == null)
            {
                continue;
            }

            copy.Status = CopyStatus.Borrowed;
            copy.Source = CopySource.Reservation;
            copy.ReservationId = reservationId;
            _unitOfWork.BoardGameCopies.Update(copy);

            game.CurrentStock -= 1;
            game.UpdatedAt = now;
            _unitOfWork.BoardGames.Update(game);
        }
    }

    /// <summary>
    /// 归还该预约借出的全部桌游副本：状态恢复在库、解除预约关联，桌游当前库存 +1
    /// </summary>
    /// <param name="reservationId">预约Id</param>
    private void ReturnReservationGames(Guid reservationId)
    {
        var copies = _unitOfWork.BoardGameCopies
            .Find(c => c.ReservationId == reservationId && c.Status == CopyStatus.Borrowed)
            .ToList();

        if (copies.Count == 0)
        {
            return;
        }

        var now = DateTime.Now;
        foreach (var copy in copies)
        {
            copy.Status = CopyStatus.InStock;
            copy.ReservationId = null;
            copy.Source = CopySource.Manual;
            _unitOfWork.BoardGameCopies.Update(copy);

            var game = _unitOfWork.BoardGames.GetById(copy.BoardGameId);
            if (game != null)
            {
                game.CurrentStock += 1;
                game.UpdatedAt = now;
                _unitOfWork.BoardGames.Update(game);
            }
        }
    }

    /// <summary>
    /// 更新预约关联桌位的状态（仅在桌位空闲/已预约时生效）
    /// </summary>
    /// <param name="tableId">桌位Id</param>
    /// <param name="targetStatus">目标状态</param>
    private void LockTable(Guid? tableId, TableStatus targetStatus)
    {
        if (!tableId.HasValue)
        {
            return;
        }

        var table = _unitOfWork.Tables.GetById(tableId.Value);
        if (table == null)
        {
            return;
        }

        var allowed = targetStatus == TableStatus.Reserved
            ? table.Status == TableStatus.Idle
            : table.Status == TableStatus.Reserved;

        if (allowed)
        {
            table.Status = targetStatus;
            table.UpdatedAt = DateTime.Now;
            _unitOfWork.Tables.Update(table);
        }
    }

    /// <summary>
    /// 根据桌游Id列表解析预约桌游名称（逗号分隔）
    /// </summary>
    /// <param name="gameIds">桌游Id列表</param>
    /// <returns>桌游名称字符串</returns>
    private string? ResolveGameNames(List<Guid> gameIds)
    {
        if (gameIds == null || gameIds.Count == 0)
        {
            return null;
        }

        var games = _unitOfWork.BoardGames
            .Find(g => gameIds.Contains(g.Id) && !g.IsDeleted)
            .ToList();
        return games.Count > 0 ? string.Join("、", games.Select(g => g.Name)) : null;
    }

    /// <summary>
    /// 计算预约押金
    /// </summary>
    /// <param name="tableType">桌位区域类型</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>押金金额</returns>
    private static decimal CalculateDeposit(TableArea tableType, TimeSpan startTime, TimeSpan endTime)
    {
        // TODO: 根据实际押金规则调整
        var hours = Math.Max(1, (int)Math.Ceiling((endTime - startTime).TotalHours));
        var baseRate = tableType switch
        {
            TableArea.PrivateRoom => 50,
            TableArea.Booth => 30,
            _ => 20
        };
        return baseRate * hours;
    }
}
