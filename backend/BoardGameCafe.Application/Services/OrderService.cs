using AutoMapper;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Common;
using BoardGameCafe.Common.Exceptions;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoardGameCafe.Application.Services;

/// <summary>
/// 订单服务实现
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// 初始化订单服务
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PagedResult<OrderDto>> GetPagedList(OrderQueryParams queryParams)
    {
        // TODO: 实现订单分页查询
        // 1. 应用Keyword、Status、日期范围、会员Id筛选
        // 2. 关联会员表和桌位表获取名称
        // 3. 应用排序和分页

        var query = _unitOfWork.Orders.GetAll().AsQueryable();

        if (queryParams.Status.HasValue)
        {
            query = query.Where(o => o.Status == queryParams.Status.Value);
        }

        if (queryParams.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= queryParams.DateFrom.Value);
        }

        if (queryParams.DateTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= queryParams.DateTo.Value);
        }

        if (queryParams.MemberId.HasValue)
        {
            query = query.Where(o => o.MemberId == queryParams.MemberId.Value);
        }

        var totalCount = query.Count();
        var orders = query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<OrderDto>>(orders);

        // 填充关联名称
        var orderIds = dtos.Select(d => d.Id).ToList();
        var orderBoardGames = _unitOfWork.OrderBoardGames
            .Find(x => orderIds.Contains(x.OrderId))
            .ToList();
        var gameNames = _unitOfWork.BoardGames.GetAll()
            .Where(g => !g.IsDeleted)
            .ToDictionary(g => g.Id, g => g.Name);

        foreach (var dto in dtos)
        {
            if (dto.MemberId.HasValue)
            {
                var member = _unitOfWork.Members.GetById(dto.MemberId.Value);
                dto.MemberName = member?.NickName;
            }
            if (dto.TableId.HasValue)
            {
                var table = _unitOfWork.Tables.GetById(dto.TableId.Value);
                dto.TableCode = table?.Code;
            }
            // 拼接游玩桌游名称（含已归还记录，订单详情可追溯）
            var names = orderBoardGames
                .Where(x => x.OrderId == dto.Id)
                .Select(x => gameNames.TryGetValue(x.BoardGameId, out var n) ? n : null)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct();
            dto.BoardGameNames = string.Join("、", names);
        }

        return Task.FromResult(PagedResult<OrderDto>.Create(dtos, queryParams.PageIndex, queryParams.PageSize, totalCount));
    }

    /// <inheritdoc />
    public Task<OrderDto> GetById(Guid id)
    {
        var order = _unitOfWork.Orders.GetById(id);
        if (order == null)
        {
            throw new NotFoundException("订单不存在");
        }

        var dto = _mapper.Map<OrderDto>(order);
        if (dto.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(dto.MemberId.Value);
            dto.MemberName = member?.NickName;
        }
        if (dto.TableId.HasValue)
        {
            var table = _unitOfWork.Tables.GetById(dto.TableId.Value);
            dto.TableCode = table?.Code;
        }
        return Task.FromResult(dto);
    }

    /// <inheritdoc />
    public Task<OrderDetailDto> Pay(PaymentDto dto)
    {
        var order = _unitOfWork.Orders.GetById(dto.OrderId);
        if (order == null)
        {
            throw new NotFoundException("订单不存在");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new BusinessException("订单状态不允许支付");
        }

        var now = DateTime.Now;
        decimal finalAmount = order.TotalAmount;
        decimal totalDiscount = 0;

        // 0. 应用折扣折数：折数折扣金额 = 总金额 × (1 - 折数/10)，实收 = 总金额 × 折数/10
        if (order.DiscountRate < 10)
        {
            totalDiscount += Math.Round(order.TotalAmount * (1m - order.DiscountRate / 10m), 2);
        }

        // 1. 处理优惠券
        if (dto.CouponId.HasValue)
        {
            var coupon = _unitOfWork.Coupons.GetById(dto.CouponId.Value);
            if (coupon != null && !coupon.IsUsed)
            {
                if (coupon.MinAmount > 0 && order.TotalAmount < coupon.MinAmount)
                {
                    throw new BusinessException($"订单金额需满{coupon.MinAmount}元才能使用该优惠券");
                }

                if (coupon.DiscountAmount.HasValue && coupon.DiscountAmount.Value > 0)
                {
                    totalDiscount += coupon.DiscountAmount.Value;
                }
                else if (coupon.Discount.HasValue && coupon.Discount.Value > 0)
                {
                    var discount = order.TotalAmount * coupon.Discount.Value;
                    totalDiscount += discount;
                }

                coupon.IsUsed = true;
                coupon.UsedOrderId = order.Id;
            }
        }

        // 2. 处理积分抵扣（100积分 = 1元）
        if (dto.PointsDeduct.HasValue && dto.PointsDeduct.Value > 0 && order.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(order.MemberId.Value);
            if (member != null)
            {
                int pointsToUse = Math.Min(dto.PointsDeduct.Value, member.Points);
                decimal pointsDeductAmount = pointsToUse / 100m;
                totalDiscount += pointsDeductAmount;
                member.Points -= pointsToUse;
                _unitOfWork.Members.Update(member);
            }
        }

        // 3. 处理余额支付 / 余额抵扣
        if (dto.PaymentMethod == PaymentMethod.MemberBalance)
        {
            // 纯余额支付：按剩余应付金额自动扣减会员余额（忽略前端传入的 balanceDeduct，避免多扣），不足时拒绝支付
            var member = order.MemberId.HasValue ? _unitOfWork.Members.GetById(order.MemberId.Value) : null;
            if (member == null)
            {
                throw new BusinessException("会员不存在，无法使用余额支付");
            }

            var remaining = Math.Max(0, order.TotalAmount - totalDiscount);
            if (member.Balance < remaining)
            {
                throw new BusinessException($"余额不足，当前余额：{member.Balance:F2}元");
            }

            member.Balance -= remaining;
            member.UpdatedAt = now;
            _unitOfWork.Members.Update(member);
            AddBalanceRecord(member.Id, -remaining, order.Id, $"消费扣款 订单号{order.OrderNo}");
            totalDiscount += remaining;
        }
        else if (dto.BalanceDeduct.HasValue && dto.BalanceDeduct.Value > 0 && order.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(order.MemberId.Value);
            if (member != null)
            {
                decimal balanceToUse = Math.Min(dto.BalanceDeduct.Value, member.Balance);
                totalDiscount += balanceToUse;
                member.Balance -= balanceToUse;
                member.UpdatedAt = now;
                _unitOfWork.Members.Update(member);
                AddBalanceRecord(member.Id, -balanceToUse, order.Id, $"消费扣款 订单号{order.OrderNo}");
            }
        }

        // 确保折扣不超过总金额
        totalDiscount = Math.Min(totalDiscount, order.TotalAmount);
        finalAmount = order.TotalAmount - totalDiscount;

        order.Status = OrderStatus.Paid;
        order.PaymentMethod = dto.PaymentMethod;
        order.PaidAt = now;
        order.UpdatedAt = now;
        order.Discount = totalDiscount;
        order.ActualAmount = finalAmount;

        _unitOfWork.Orders.Update(order);
        _unitOfWork.SaveChanges();

        var detailDto = _mapper.Map<OrderDetailDto>(order);
        var orderItems = _unitOfWork.OrderItems.Find(i => i.OrderId == order.Id).ToList();
        detailDto.OrderItems = _mapper.Map<List<OrderItemDto>>(orderItems);

        if (detailDto.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(detailDto.MemberId.Value);
            detailDto.MemberName = member?.NickName;
        }
        if (detailDto.TableId.HasValue)
        {
            var table = _unitOfWork.Tables.GetById(detailDto.TableId.Value);
            detailDto.TableCode = table?.Code;
        }

        return Task.FromResult(detailDto);
    }

    /// <inheritdoc />
    public Task Refund(Guid id, Guid? operatorId)
    {
        var order = _unitOfWork.Orders.GetById(id);
        if (order == null)
        {
            throw new NotFoundException("订单不存在");
        }

        if (order.Status != OrderStatus.Paid)
        {
            throw new BusinessException("只有已支付的订单才能退款");
        }

        var now = DateTime.Now;
        order.Status = OrderStatus.Refunded;
        order.UpdatedAt = now;

        // 退回会员余额（毕设简化：余额支付按实际扣款退回，其他支付按实收退回；散客订单仅改状态）
        decimal refundAmount = 0;
        if (order.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(order.MemberId.Value);
            if (member != null)
            {
                if (order.PaymentMethod == PaymentMethod.MemberBalance)
                {
                    // 余额支付：退回实际扣除的余额（按该订单的消费流水）
                    var deducted = _unitOfWork.MemberBalanceRecords
                        .Find(r => r.OrderId == order.Id && r.Type == "Consume")
                        .Sum(r => (decimal?)r.Amount) ?? 0m;
                    refundAmount = -deducted;
                }
                else
                {
                    refundAmount = order.ActualAmount;
                }

                if (refundAmount > 0)
                {
                    member.Balance += refundAmount;
                    member.UpdatedAt = now;
                    _unitOfWork.Members.Update(member);
                    AddBalanceRecord(member.Id, refundAmount, order.Id, $"订单退款 订单号{order.OrderNo} 金额¥{refundAmount:F2}", "Refund");
                }
            }
        }

        // 归还开台借出的游玩桌游（库存 +1，幂等）及订单关联预约借出的桌游
        ReturnOrderGames(order);
        ReturnReservationGames(order);

        // 桌位状态恢复为空闲（仅当桌位当前为占用中且关联该订单）
        if (order.TableId.HasValue)
        {
            var table = _unitOfWork.Tables.GetById(order.TableId.Value);
            if (table != null && table.Status == TableStatus.Occupied)
            {
                table.Status = TableStatus.Idle;
                table.UpdatedAt = now;
                _unitOfWork.Tables.Update(table);
            }
        }

        // 记录操作日志
        var operatorName = operatorId.HasValue
            ? _unitOfWork.Users.GetById(operatorId.Value)?.UserName ?? string.Empty
            : string.Empty;
        _unitOfWork.OperationLogs.Add(new OperationLog
        {
            Id = Guid.NewGuid(),
            UserId = operatorId,
            UserName = operatorName,
            Action = "REFUND",
            TargetId = order.OrderNo,
            Description = refundAmount > 0
                ? $"退款订单 {order.OrderNo} 金额¥{refundAmount:F2}"
                : $"退款订单 {order.OrderNo}（散客订单，仅记录状态）",
            Timestamp = now
        });

        _unitOfWork.Orders.Update(order);
        _unitOfWork.SaveChanges();

        return Task.CompletedTask;
    }

    /// <summary>
    /// 归还订单关联预约借出的桌游副本：状态恢复在库、解除预约关联，桌游当前库存 +1
    /// </summary>
    /// <param name="order">订单</param>
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

    /// <inheritdoc />
    public Task<OrderDetailDto> GetDetail(Guid id)
    {
        var order = _unitOfWork.Orders.GetById(id);
        if (order == null)
        {
            throw new NotFoundException("订单不存在");
        }

        var detailDto = _mapper.Map<OrderDetailDto>(order);
        var orderItems = _unitOfWork.OrderItems.Find(i => i.OrderId == id).ToList();
        detailDto.OrderItems = _mapper.Map<List<OrderItemDto>>(orderItems);

        // 填充游玩桌游列表（含已归还记录，订单详情可追溯）
        var orderBoardGames = _unitOfWork.OrderBoardGames
            .Find(x => x.OrderId == id)
            .ToList();
        var boardGames = _unitOfWork.BoardGames.GetAll().Where(g => !g.IsDeleted).ToList();
        foreach (var item in orderBoardGames)
        {
            var game = boardGames.FirstOrDefault(g => g.Id == item.BoardGameId);
            detailDto.BoardGames.Add(new OrderBoardGameDto
            {
                BoardGameId = item.BoardGameId,
                Name = game?.Name ?? "已下架桌游",
                GameNo = game?.GameNo ?? string.Empty,
                Quantity = item.Quantity,
                IsReturned = item.IsReturned
            });
        }

        if (detailDto.MemberId.HasValue)
        {
            var member = _unitOfWork.Members.GetById(detailDto.MemberId.Value);
            detailDto.MemberName = member?.NickName;
        }
        if (detailDto.TableId.HasValue)
        {
            var table = _unitOfWork.Tables.GetById(detailDto.TableId.Value);
            detailDto.TableCode = table?.Code;
        }

        return Task.FromResult(detailDto);
    }

    /// <summary>
    /// 归还开台借出的游玩桌游：订单关联的桌游标记已归还，桌游当前库存 +数量（幂等）
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
    /// 记录会员余额变动流水
    /// </summary>
    /// <param name="memberId">会员Id</param>
    /// <param name="amount">变动金额（消费为负，退款为正）</param>
    /// <param name="orderId">关联订单Id</param>
    /// <param name="description">流水说明</param>
    /// <param name="type">流水类型（默认消费扣款）</param>
    private void AddBalanceRecord(Guid memberId, decimal amount, Guid? orderId, string description, string type = "Consume")
    {
        _unitOfWork.MemberBalanceRecords.Add(new MemberBalanceRecord
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Type = type,
            Amount = amount,
            OrderId = orderId,
            Description = description,
            CreatedAt = DateTime.Now
        });
    }
}
