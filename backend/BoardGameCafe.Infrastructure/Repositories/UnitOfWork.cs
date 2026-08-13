using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Interfaces;
using BoardGameCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BoardGameCafe.Infrastructure.Repositories;

/// <summary>
/// 工作单元实现
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    private readonly BoardGameCafeDbContext _context;

    /// <summary>
    /// 当前事务
    /// </summary>
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// 用户仓储（延迟初始化）
    /// </summary>
    private IRepository<User>? _users;

    /// <summary>
    /// 会员仓储（延迟初始化）
    /// </summary>
    private IRepository<Member>? _members;

    /// <summary>
    /// 桌游仓储（延迟初始化）
    /// </summary>
    private IRepository<BoardGame>? _boardGames;

    /// <summary>
    /// 桌位仓储（延迟初始化）
    /// </summary>
    private IRepository<Table>? _tables;

    /// <summary>
    /// 订单仓储（延迟初始化）
    /// </summary>
    private IRepository<Order>? _orders;

    /// <summary>
    /// 订单项仓储（延迟初始化）
    /// </summary>
    private IRepository<OrderItem>? _orderItems;

    /// <summary>
    /// 订单-桌游关联仓储（延迟初始化）
    /// </summary>
    private IRepository<OrderBoardGame>? _orderBoardGames;

    /// <summary>
    /// 预约仓储（延迟初始化）
    /// </summary>
    private IRepository<Reservation>? _reservations;

    /// <summary>
    /// 损坏记录仓储（延迟初始化）
    /// </summary>
    private IRepository<DamageRecord>? _damageRecords;

    /// <summary>
    /// 操作日志仓储（延迟初始化）
    /// </summary>
    private IRepository<OperationLog>? _operationLogs;

    /// <summary>
    /// 充值记录仓储（延迟初始化）
    /// </summary>
    private IRepository<RechargeRecord>? _rechargeRecords;

    /// <summary>
    /// 优惠券仓储（延迟初始化）
    /// </summary>
    private IRepository<Coupon>? _coupons;

    /// <summary>
    /// 桌游副本仓储（延迟初始化）
    /// </summary>
    private IRepository<BoardGameCopy>? _boardGameCopies;

    /// <summary>
    /// 补货记录仓储（延迟初始化）
    /// </summary>
    private IRepository<RestockRecord>? _restockRecords;

    /// <summary>
    /// 会员余额流水仓储（延迟初始化）
    /// </summary>
    private IRepository<MemberBalanceRecord>? _memberBalanceRecords;

    /// <summary>
    /// 释放资源标记
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// 初始化工作单元
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public UnitOfWork(BoardGameCafeDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 用户仓储
    /// </summary>
    public IRepository<User> Users
    {
        get { return _users ??= new Repository<User>(_context); }
    }

    /// <summary>
    /// 会员仓储
    /// </summary>
    public IRepository<Member> Members
    {
        get { return _members ??= new Repository<Member>(_context); }
    }

    /// <summary>
    /// 桌游仓储
    /// </summary>
    public IRepository<BoardGame> BoardGames
    {
        get { return _boardGames ??= new Repository<BoardGame>(_context); }
    }

    /// <summary>
    /// 桌位仓储
    /// </summary>
    public IRepository<Table> Tables
    {
        get { return _tables ??= new Repository<Table>(_context); }
    }

    /// <summary>
    /// 订单仓储
    /// </summary>
    public IRepository<Order> Orders
    {
        get { return _orders ??= new Repository<Order>(_context); }
    }

    /// <summary>
    /// 订单项仓储
    /// </summary>
    public IRepository<OrderItem> OrderItems
    {
        get { return _orderItems ??= new Repository<OrderItem>(_context); }
    }

    /// <summary>
    /// 订单-桌游关联仓储
    /// </summary>
    public IRepository<OrderBoardGame> OrderBoardGames
    {
        get { return _orderBoardGames ??= new Repository<OrderBoardGame>(_context); }
    }

    /// <summary>
    /// 预约仓储
    /// </summary>
    public IRepository<Reservation> Reservations
    {
        get { return _reservations ??= new Repository<Reservation>(_context); }
    }

    /// <summary>
    /// 损坏记录仓储
    /// </summary>
    public IRepository<DamageRecord> DamageRecords
    {
        get { return _damageRecords ??= new Repository<DamageRecord>(_context); }
    }

    /// <summary>
    /// 操作日志仓储
    /// </summary>
    public IRepository<OperationLog> OperationLogs
    {
        get { return _operationLogs ??= new Repository<OperationLog>(_context); }
    }

    /// <summary>
    /// 充值记录仓储
    /// </summary>
    public IRepository<RechargeRecord> RechargeRecords
    {
        get { return _rechargeRecords ??= new Repository<RechargeRecord>(_context); }
    }

    /// <summary>
    /// 优惠券仓储
    /// </summary>
    public IRepository<Coupon> Coupons
    {
        get { return _coupons ??= new Repository<Coupon>(_context); }
    }

    /// <summary>
    /// 桌游副本仓储
    /// </summary>
    public IRepository<BoardGameCopy> BoardGameCopies
    {
        get { return _boardGameCopies ??= new Repository<BoardGameCopy>(_context); }
    }

    /// <summary>
    /// 补货记录仓储
    /// </summary>
    public IRepository<RestockRecord> RestockRecords
    {
        get { return _restockRecords ??= new Repository<RestockRecord>(_context); }
    }

    /// <summary>
    /// 会员余额流水仓储
    /// </summary>
    public IRepository<MemberBalanceRecord> MemberBalanceRecords
    {
        get { return _memberBalanceRecords ??= new Repository<MemberBalanceRecord>(_context); }
    }

    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>受影响的行数</returns>
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    /// <summary>
    /// 开始事务
    /// </summary>
    public void BeginTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void Commit()
    {
        try
        {
            _context.SaveChanges();
            _transaction?.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
