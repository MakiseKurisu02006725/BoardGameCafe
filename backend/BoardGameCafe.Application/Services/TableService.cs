using AutoMapper;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common.Exceptions;
using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 桌位服务实现
/// </summary>
public class TableService : ITableService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TableService> _logger;

    /// <summary>
    /// 初始化桌位服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public TableService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TableService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<List<TableDto>> GetAll()
    {
        var tables = _unitOfWork.Tables.GetAll().OrderBy(t => t.Code).ToList();
        var dtos = _mapper.Map<List<TableDto>>(tables);

        // 已预约桌位：补充关联预约信息（预约Id/联系人/人数），供实时看板"确认入座"使用
        var reservedTableIds = dtos
            .Where(d => d.Status == TableStatus.Reserved)
            .Select(d => d.Id)
            .ToList();
        if (reservedTableIds.Count > 0)
        {
            var reservations = _unitOfWork.Reservations
                .Find(r => r.TableId.HasValue && reservedTableIds.Contains(r.TableId.Value) &&
                           r.Status == ReservationStatus.Confirmed)
                .ToList();
            foreach (var dto in dtos)
            {
                var resv = reservations.FirstOrDefault(r => r.TableId == dto.Id);
                if (resv == null)
                {
                    continue;
                }

                dto.ReservationId = resv.Id;
                dto.PeopleCount = resv.GuestCount;
                dto.ReservedTime = $"{resv.StartTime.ToString(@"hh\:mm")}-{resv.EndTime.ToString(@"hh\:mm")}";
                dto.ReservedName = _unitOfWork.Members.GetById(resv.MemberId)?.NickName ?? string.Empty;
            }
        }

        // 占用中桌位：补充当前订单信息（订单Id/开台时间/会员），供实时看板结账使用
        // 取该桌位最新一笔订单（不限状态）：已支付订单走 CloseTable 的关台分支，待支付订单走完整支付分支
        var occupiedTableIds = dtos
            .Where(d => d.Status == TableStatus.Occupied)
            .Select(d => d.Id)
            .ToList();
        if (occupiedTableIds.Count > 0)
        {
            var latestOrders = _unitOfWork.Orders
                .Find(o => occupiedTableIds.Contains(o.TableId ?? Guid.Empty))
                .ToList()
                .GroupBy(o => o.TableId ?? Guid.Empty)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.StartTime).First());
            foreach (var dto in dtos.Where(d => d.Status == TableStatus.Occupied))
            {
                if (!latestOrders.TryGetValue(dto.Id, out var order))
                {
                    continue;
                }

                dto.OrderId = order.Id;
                dto.OpenTime = order.StartTime;
                dto.MemberName = order.MemberId.HasValue
                    ? _unitOfWork.Members.GetById(order.MemberId.Value)?.NickName ?? string.Empty
                    : string.Empty;
            }
        }

        // 填充 AutoMapper 无法自动映射的 AreaName / StatusName
        foreach (var dto in dtos)
        {
            dto.AreaName = dto.Area switch
            {
                TableArea.Hall => "大厅",
                TableArea.PrivateRoom => "包间",
                TableArea.Booth => "卡座",
                _ => "未知"
            };

            dto.StatusName = dto.Status switch
            {
                TableStatus.Idle => "空闲",
                TableStatus.Occupied => "占用中",
                TableStatus.Reserved => "已预约",
                TableStatus.Maintenance => "维修",
                _ => "未知"
            };
        }

        return Task.FromResult(dtos);
    }

    /// <inheritdoc />
    public Task<TableDto> GetById(Guid id)
    {
        var table = _unitOfWork.Tables.GetById(id);
        if (table == null)
        {
            throw new NotFoundException("桌位不存在");
        }

        var dto = _mapper.Map<TableDto>(table);
        dto.AreaName = dto.Area switch
        {
            TableArea.Hall => "大厅",
            TableArea.PrivateRoom => "包间",
            TableArea.Booth => "卡座",
            _ => "未知"
        };
        dto.StatusName = dto.Status switch
        {
            TableStatus.Idle => "空闲",
            TableStatus.Occupied => "占用中",
            TableStatus.Reserved => "已预约",
            _ => "未知"
        };

        return Task.FromResult(dto);
    }

    /// <inheritdoc />
    public Task<TableDto> Create(CreateTableDto dto)
    {
        // TODO: 检查桌位编号是否重复
        var existing = _unitOfWork.Tables.Find(t => t.Code == dto.Code).FirstOrDefault();
        if (existing != null)
        {
            throw new BusinessException("桌位编号已存在");
        }

        var now = DateTime.Now;
        var table = _mapper.Map<Table>(dto);
        table.Id = Guid.NewGuid();
        table.CreatedAt = now;
        table.UpdatedAt = now;

        _unitOfWork.Tables.Add(table);
        _unitOfWork.SaveChanges();

        var resultDto = _mapper.Map<TableDto>(table);
        resultDto.AreaName = resultDto.Area switch
        {
            TableArea.Hall => "大厅",
            TableArea.PrivateRoom => "包间",
            TableArea.Booth => "卡座",
            _ => "未知"
        };
        resultDto.StatusName = resultDto.Status switch
        {
            TableStatus.Idle => "空闲",
            TableStatus.Occupied => "占用中",
            TableStatus.Reserved => "已预约",
            _ => "未知"
        };
        return Task.FromResult(resultDto);
    }

    /// <inheritdoc />
    public Task Update(Guid id, UpdateTableDto dto)
    {
        var table = _unitOfWork.Tables.GetById(id);
        if (table == null)
        {
            throw new NotFoundException("桌位不存在");
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
            table.Code = dto.Code;
        if (dto.Area.HasValue)
            table.Area = dto.Area.Value;
        if (dto.Seats.HasValue)
            table.Seats = dto.Seats.Value;
        if (dto.HourlyRate.HasValue)
            table.HourlyRate = dto.HourlyRate.Value;
        if (dto.Status.HasValue)
            table.Status = dto.Status.Value;

        table.UpdatedAt = DateTime.Now;
        _unitOfWork.Tables.Update(table);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Delete(Guid id)
    {
        // TODO: 检查是否有关联的进行中订单
        var table = _unitOfWork.Tables.GetById(id);
        if (table == null)
        {
            throw new NotFoundException("桌位不存在");
        }

        _unitOfWork.Tables.Delete(table);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OrderDto> OpenTable(OpenTableDto dto, Guid createdBy)
    {
        // TODO: 实现开台逻辑
        // 1. 检查桌位状态是否为空闲
        // 2. 更新桌位状态为占用中
        // 3. 创建订单（状态=待支付）
        // 4. 保存更改

        var table = _unitOfWork.Tables.GetById(dto.TableId);
        if (table == null)
        {
            throw new NotFoundException("桌位不存在");
        }

        if (table.Status != TableStatus.Idle)
        {
            throw new BusinessException("该桌位当前不可用");
        }

        // 校验折扣折数合法性：0 < 折扣 ≤ 10
        var discountRate = dto.Discount ?? 10m;
        if (discountRate <= 0 || discountRate > 10)
        {
            throw new BusinessException("折扣范围仅支持0.1~10.0折");
        }

        // 校验游玩桌游：必须至少选择一款
        var boardGameIds = dto.BoardGameIds?.Distinct().ToList() ?? new List<Guid>();
        if (boardGameIds.Count == 0)
        {
            throw new BusinessException("请至少选择一款游玩桌游");
        }

        // 校验桌游库存充足（每款借出 1 份，扣减当前库存）
        foreach (var gameId in boardGameIds)
        {
            var game = _unitOfWork.BoardGames.GetById(gameId);
            if (game == null || game.IsDeleted)
            {
                throw new NotFoundException("桌游不存在");
            }
            if (game.CurrentStock < 1)
            {
                throw new BusinessException($"桌游「{game.Name}」库存不足，无法借出");
            }
        }

        var now = DateTime.Now;
        table.Status = TableStatus.Occupied;
        table.UpdatedAt = now;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNo = GenerateOrderNo(),
            MemberId = dto.MemberId,
            TableId = dto.TableId,
            StartTime = now,
            EndTime = null,
            TotalAmount = 0,
            DiscountRate = discountRate,
            Discount = 0,
            ActualAmount = 0,
            Status = OrderStatus.Pending,
            PaymentMethod = null,
            CreatedBy = createdBy,
            PaidAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        // 扣减游玩桌游库存并登记订单-桌游关联记录（结账/退款后自动归还）
        foreach (var gameId in boardGameIds)
        {
            var game = _unitOfWork.BoardGames.GetById(gameId);
            if (game == null)
            {
                continue;
            }
            game.CurrentStock -= 1;
            game.UpdatedAt = now;
            _unitOfWork.BoardGames.Update(game);

            _unitOfWork.OrderBoardGames.Add(new OrderBoardGame
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                BoardGameId = gameId,
                Quantity = 1,
                IsReturned = false,
                CreatedAt = now
            });
        }

        _unitOfWork.Tables.Update(table);
        _unitOfWork.Orders.Add(order);
        _unitOfWork.SaveChanges();

        var orderDto = _mapper.Map<OrderDto>(order);
        orderDto.TableCode = table.Code;
        if (dto.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(dto.MemberId.Value);
            orderDto.MemberName = member?.NickName;
        }
        return Task.FromResult(orderDto);
    }

    /// <inheritdoc />
    public Task ChangeTable(ChangeTableDto dto)
    {
        // TODO: 实现换台逻辑
        // 1. 检查当前订单是否存在且进行中
        // 2. 检查新桌位是否空闲
        // 3. 更新旧桌位状态为空闲
        // 4. 更新新桌位状态为占用中
        // 5. 更新订单关联的桌位Id
        // 6. 保存更改

        var order = _unitOfWork.Orders.GetById(dto.CurrentOrderId);
        if (order == null || order.Status != OrderStatus.Pending)
        {
            throw new BusinessException("订单不存在或已结账");
        }

        var oldTable = order.TableId.HasValue ? _unitOfWork.Tables.GetById(order.TableId.Value) : null;
        var newTable = _unitOfWork.Tables.GetById(dto.NewTableId);

        if (newTable == null)
        {
            throw new NotFoundException("新桌位不存在");
        }

        if (newTable.Status != TableStatus.Idle)
        {
            throw new BusinessException("新桌位当前不可用");
        }

        var now = DateTime.Now;

        if (oldTable != null)
        {
            oldTable.Status = TableStatus.Idle;
            oldTable.UpdatedAt = now;
            _unitOfWork.Tables.Update(oldTable);
        }

        newTable.Status = TableStatus.Occupied;
        newTable.UpdatedAt = now;

        order.TableId = dto.NewTableId;
        order.UpdatedAt = now;

        _unitOfWork.Tables.Update(newTable);
        _unitOfWork.Orders.Update(order);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OrderDetailDto> CloseTable(CloseTableDto dto)
    {
        var order = _unitOfWork.Orders.GetById(dto.OrderId);
        if (order == null)
        {
            throw new BusinessException("订单不存在");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new BusinessException("订单已取消");
        }

        var table = order.TableId.HasValue ? _unitOfWork.Tables.GetById(order.TableId.Value) : null;
        var now = DateTime.Now;

        // If already paid, just close the table
        if (order.Status == OrderStatus.Paid)
        {
            // 补记实际关台时间，便于订单列表计算时长
            order.EndTime = now;
            order.UpdatedAt = now;
            _unitOfWork.Orders.Update(order);

            if (table != null)
            {
                table.Status = TableStatus.Idle;
                table.UpdatedAt = now;
                _unitOfWork.Tables.Update(table);
            }

            // 结账完成：归还开台借出桌游（库存 +1）及订单关联预约借出的桌游
            ReturnOrderGames(order);
            ReturnReservationGames(order);

            _unitOfWork.SaveChanges();

            var paidDto = _mapper.Map<OrderDetailDto>(order);
            paidDto.TableCode = table?.Code;
            paidDto.OrderItems = _mapper.Map<List<OrderItemDto>>(_unitOfWork.OrderItems.Find(i => i.OrderId == order.Id).ToList());
            return Task.FromResult(paidDto);
        }

        // Full payment flow for pending orders
        order.EndTime = now;

        // 计算时长和桌位费用
        var duration = (now - order.StartTime).TotalHours;
        var hours = (decimal)Math.Ceiling(duration);
        var tableFee = table != null ? hours * table.HourlyRate : 0;

        var orderItems = _unitOfWork.OrderItems.Find(i => i.OrderId == order.Id).ToList();
        var itemsTotal = orderItems.Sum(i => i.Subtotal);

        order.TotalAmount = tableFee + itemsTotal;

        decimal discount = 0;

        // 应用折扣折数：折数折扣金额 = 总金额 × (1 - 折数/10)，实收 = 总金额 × 折数/10
        if (order.DiscountRate < 10)
        {
            discount += Math.Round(order.TotalAmount * (1m - order.DiscountRate / 10m), 2);
        }

        // 处理优惠券
        if (dto.CouponId.HasValue)
        {
            var coupon = _unitOfWork.Coupons.GetById(dto.CouponId.Value);
            if (coupon != null && !coupon.IsUsed && order.TotalAmount >= coupon.MinAmount)
            {
                coupon.IsUsed = true;
                coupon.UsedOrderId = order.Id;
                _unitOfWork.Coupons.Update(coupon);
                discount += coupon.DiscountAmount ?? 0;
            }
        }

        // 处理积分抵扣（100积分 = 1元）
        if (dto.PointsDeduct.HasValue && order.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(order.MemberId.Value);
            if (member != null && member.Points >= dto.PointsDeduct.Value)
            {
                member.Points -= dto.PointsDeduct.Value;
                _unitOfWork.Members.Update(member);
                discount += dto.PointsDeduct.Value / 100m;
            }
        }

        // 处理余额支付 / 余额抵扣
        decimal balanceDeduct = 0;
        if (dto.PaymentMethod == PaymentMethod.MemberBalance)
        {
            // 纯余额支付：按剩余应付金额自动扣减会员余额（忽略前端传入的 balanceDeduct，避免多扣），不足时拒绝支付
            var member = order.MemberId.HasValue ? _unitOfWork.Members.GetById(order.MemberId.Value) : null;
            if (member == null)
            {
                throw new BusinessException("会员不存在，无法使用余额支付");
            }

            var remaining = Math.Max(0, order.TotalAmount - discount);
            if (member.Balance < remaining)
            {
                throw new BusinessException($"余额不足，当前余额：{member.Balance:F2}元");
            }

            member.Balance -= remaining;
            member.UpdatedAt = now;
            _unitOfWork.Members.Update(member);
            AddBalanceRecord(member.Id, -remaining, order.Id, $"消费扣款 订单号{order.OrderNo}");
            balanceDeduct = remaining;
            discount += balanceDeduct;
        }
        else if (dto.BalanceDeduct.HasValue && order.MemberId.HasValue)
        {
            // 混合支付中的余额抵扣（微信/支付宝/现金 + 余额）：抵扣金额不超过会员余额
            var member = _unitOfWork.Members.GetById(order.MemberId.Value);
            if (member != null && member.Balance >= dto.BalanceDeduct.Value)
            {
                member.Balance -= dto.BalanceDeduct.Value;
                member.UpdatedAt = now;
                _unitOfWork.Members.Update(member);
                AddBalanceRecord(member.Id, -dto.BalanceDeduct.Value, order.Id, $"消费扣款 订单号{order.OrderNo}");
                balanceDeduct = dto.BalanceDeduct.Value;
                discount += balanceDeduct;
            }
        }

        order.Discount = discount;
        order.ActualAmount = Math.Max(0, order.TotalAmount - order.Discount);
        order.Status = OrderStatus.Paid;
        order.PaymentMethod = dto.PaymentMethod;
        order.PaidAt = now;
        order.UpdatedAt = now;

        // 添加桌位费订单项
        if (tableFee > 0 && table != null)
        {
            var tableFeeItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Name = "桌位费",
                Quantity = 1,
                UnitPrice = table.HourlyRate,
                Type = OrderItemType.TableFee,
                Subtotal = tableFee,
                CreatedAt = now
            };
            _unitOfWork.OrderItems.Add(tableFeeItem);
            orderItems.Add(tableFeeItem);
        }

        if (table != null)
        {
            table.Status = TableStatus.Idle;
            table.UpdatedAt = now;
            _unitOfWork.Tables.Update(table);
        }

        _unitOfWork.Orders.Update(order);
        SaveEarnedPoints(order, now);

        // 结账完成：归还开台借出桌游（库存 +1）及订单关联预约借出的桌游
        ReturnOrderGames(order);
        ReturnReservationGames(order);

        _unitOfWork.SaveChanges();

        var detailDto = _mapper.Map<OrderDetailDto>(order);
        detailDto.TableCode = table?.Code;
        detailDto.OrderItems = _mapper.Map<List<OrderItemDto>>(orderItems);
        return Task.FromResult(detailDto);
    }

    /// <inheritdoc />
    public Task Maintain(Guid id, string status, Guid? operatorId)
    {
        var table = _unitOfWork.Tables.GetById(id);
        if (table == null)
        {
            throw new NotFoundException("桌位不存在");
        }

        var toMaintenance = string.Equals(status, "maintain", StringComparison.OrdinalIgnoreCase);
        if (toMaintenance)
        {
            // 仅空闲桌位可设置维修，占用/预约桌位禁止直接切换
            if (table.Status != TableStatus.Idle)
            {
                throw new BusinessException("占用/预约状态桌位不能设置维修，请先结束订单/取消预约");
            }

            table.Status = TableStatus.Maintenance;
        }
        else
        {
            // 仅维修桌位可解除维修，恢复为空闲
            if (table.Status != TableStatus.Maintenance)
            {
                throw new BusinessException("当前桌位不是维修状态，无需解除维修");
            }

            table.Status = TableStatus.Idle;
        }

        table.UpdatedAt = DateTime.Now;
        _unitOfWork.Tables.Update(table);

        // 记录操作日志：操作人、桌位编号、操作内容
        var operatorName = operatorId.HasValue
            ? _unitOfWork.Users.GetById(operatorId.Value)?.UserName ?? string.Empty
            : string.Empty;
        _unitOfWork.OperationLogs.Add(new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = operatorId,
            UserName = operatorName,
            Action = "UPDATE",
            TargetId = table.Code,
            Description = toMaintenance ? $"设置维修 桌位{table.Code}" : $"解除维修 桌位{table.Code}",
            Timestamp = DateTime.Now
        });

        _unitOfWork.SaveChanges();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 消费赠送积分：按实付金额 × 等级倍数（向下取整），并记录操作日志
    /// </summary>
    /// <param name="order">已支付的订单</param>
    /// <param name="now">当前时间</param>
    private void SaveEarnedPoints(Order order, DateTime now)
    {
        if (!order.MemberId.HasValue || order.ActualAmount <= 0)
        {
            return;
        }

        var member = _unitOfWork.Members.GetById(order.MemberId.Value);
        if (member == null)
        {
            return;
        }

        var multiplier = member.Level switch
        {
            MemberLevel.Silver => 1.2m,
            MemberLevel.Gold => 1.5m,
            MemberLevel.Diamond => 2.0m,
            _ => 1.0m
        };

        var earnedPoints = (int)Math.Floor(order.ActualAmount * multiplier);
        if (earnedPoints <= 0)
        {
            return;
        }

        member.Points += earnedPoints;
        member.UpdatedAt = now;
        _unitOfWork.Members.Update(member);

        _unitOfWork.OperationLogs.Add(new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = order.CreatedBy == Guid.Empty ? null : order.CreatedBy,
            UserName = "系统",
            Action = "UPDATE",
            TargetId = string.IsNullOrEmpty(member.MemberNo) ? null : member.MemberNo,
            Description = $"消费获得 {earnedPoints} 积分",
            Timestamp = now
        });
    }

    /// <summary>
    /// 记录会员余额变动流水（消费扣款）
    /// </summary>
    /// <param name="memberId">会员Id</param>
    /// <param name="amount">扣款金额（负数）</param>
    /// <param name="orderId">关联订单Id</param>
    /// <param name="description">流水说明</param>
    private void AddBalanceRecord(Guid memberId, decimal amount, Guid? orderId, string description)
    {
        _unitOfWork.MemberBalanceRecords.Add(new MemberBalanceRecord
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Type = "Consume",
            Amount = amount,
            OrderId = orderId,
            Description = description,
            CreatedAt = DateTime.Now
        });
    }

    /// <summary>
    /// 结账完成后归还预约借出的桌游：订单关联预约（Order.ReservationId）借出的副本恢复在库，桌游当前库存 +1
    /// </summary>
    /// <param name="order">已支付订单</param>
    private void ReturnReservationGames(Order order)
    {
        if (!order.ReservationId.HasValue)
        {
            return;
        }

        var copies = _unitOfWork.BoardGameCopies
            .Find(c => c.ReservationId == order.ReservationId.Value && c.Status == CopyStatus.Borrowed)
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
    /// 归还开台借出的桌游：订单关联的游玩桌游标记已归还，桌游当前库存 +数量（幂等）
    /// </summary>
    /// <param name="order">订单</param>
    private void ReturnOrderGames(Order order)
    {
        var items = _unitOfWork.OrderBoardGames
            .Find(x => x.OrderId == order.Id && !x.IsReturned)
            .ToList();
        if (items.Count == 0)
        {
            return;
        }

        var now = DateTime.Now;
        foreach (var item in items)
        {
            item.IsReturned = true;
            _unitOfWork.OrderBoardGames.Update(item);

            var game = _unitOfWork.BoardGames.GetById(item.BoardGameId);
            if (game != null)
            {
                game.CurrentStock += item.Quantity;
                game.UpdatedAt = now;
                _unitOfWork.BoardGames.Update(game);
            }
        }
    }

    /// <summary>
    /// 生成当天订单编号（ORD + yyyyMMdd + 3位当日序号）
    /// </summary>
    /// <returns>新订单号</returns>
    private string GenerateOrderNo()
    {
        var prefix = "ORD" + DateTime.Now.ToString("yyyyMMdd");
        var todayOrderNos = _unitOfWork.Orders
            .Find(o => o.OrderNo.StartsWith(prefix))
            .Select(o => o.OrderNo);
        return BusinessNoGenerator.NextOrderNo(todayOrderNos);
    }
}
