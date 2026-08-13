using BoardGameCafe.Core.Entities;

namespace BoardGameCafe.Core.Interfaces;

/// <summary>
/// 工作单元接口
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 用户仓储
    /// </summary>
    IRepository<User> Users { get; }

    /// <summary>
    /// 会员仓储
    /// </summary>
    IRepository<Member> Members { get; }

    /// <summary>
    /// 桌游仓储
    /// </summary>
    IRepository<BoardGame> BoardGames { get; }

    /// <summary>
    /// 桌位仓储
    /// </summary>
    IRepository<Table> Tables { get; }

    /// <summary>
    /// 订单仓储
    /// </summary>
    IRepository<Order> Orders { get; }

    /// <summary>
    /// 订单项仓储
    /// </summary>
    IRepository<OrderItem> OrderItems { get; }

    /// <summary>
    /// 订单-桌游关联仓储（开台选择的游玩桌游）
    /// </summary>
    IRepository<OrderBoardGame> OrderBoardGames { get; }

    /// <summary>
    /// 预约仓储
    /// </summary>
    IRepository<Reservation> Reservations { get; }

    /// <summary>
    /// 损坏记录仓储
    /// </summary>
    IRepository<DamageRecord> DamageRecords { get; }

    /// <summary>
    /// 操作日志仓储
    /// </summary>
    IRepository<OperationLog> OperationLogs { get; }

    /// <summary>
    /// 充值记录仓储
    /// </summary>
    IRepository<RechargeRecord> RechargeRecords { get; }

    /// <summary>
    /// 优惠券仓储
    /// </summary>
    IRepository<Coupon> Coupons { get; }

    /// <summary>
    /// 桌游副本仓储
    /// </summary>
    IRepository<BoardGameCopy> BoardGameCopies { get; }

    /// <summary>
    /// 补货记录仓储
    /// </summary>
    IRepository<RestockRecord> RestockRecords { get; }

    /// <summary>
    /// 会员余额流水仓储
    /// </summary>
    IRepository<MemberBalanceRecord> MemberBalanceRecords { get; }

    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>受影响的行数</returns>
    int SaveChanges();

    /// <summary>
    /// 开始事务
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// 提交事务
    /// </summary>
    void Commit();

    /// <summary>
    /// 回滚事务
    /// </summary>
    void Rollback();
}
