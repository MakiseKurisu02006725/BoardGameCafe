using BoardGameCafe.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGameCafe.Infrastructure.Data;

/// <summary>
/// 桌游咖啡馆数据库上下文
/// </summary>
public class BoardGameCafeDbContext : DbContext
{
    /// <summary>
    /// 初始化数据库上下文
    /// </summary>
    /// <param name="options">数据库上下文配置选项</param>
    public BoardGameCafeDbContext(DbContextOptions<BoardGameCafeDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 用户数据集
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 会员数据集
    /// </summary>
    public DbSet<Member> Members { get; set; }

    /// <summary>
    /// 桌游数据集
    /// </summary>
    public DbSet<BoardGame> BoardGames { get; set; }

    /// <summary>
    /// 桌位数据集
    /// </summary>
    public DbSet<Table> Tables { get; set; }

    /// <summary>
    /// 订单数据集
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// 订单项数据集
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// 订单-桌游关联数据集（开台选择的游玩桌游）
    /// </summary>
    public DbSet<OrderBoardGame> OrderBoardGames { get; set; }

    /// <summary>
    /// 预约数据集
    /// </summary>
    public DbSet<Reservation> Reservations { get; set; }

    /// <summary>
    /// 损坏记录数据集
    /// </summary>
    public DbSet<DamageRecord> DamageRecords { get; set; }

    /// <summary>
    /// 操作日志数据集
    /// </summary>
    public DbSet<OperationLog> OperationLogs { get; set; }

    /// <summary>
    /// 充值记录数据集
    /// </summary>
    public DbSet<RechargeRecord> RechargeRecords { get; set; }

    /// <summary>
    /// 优惠券数据集
    /// </summary>
    public DbSet<Coupon> Coupons { get; set; }

    /// <summary>
    /// 桌游副本数据集
    /// </summary>
    public DbSet<BoardGameCopy> BoardGameCopies { get; set; }

    /// <summary>
    /// 补货记录数据集
    /// </summary>
    public DbSet<RestockRecord> RestockRecords { get; set; }

    /// <summary>
    /// 会员余额流水数据集
    /// </summary>
    public DbSet<MemberBalanceRecord> MemberBalanceRecords { get; set; }

    /// <summary>
    /// 系统配置数据集
    /// </summary>
    public DbSet<SystemSetting> SystemSettings { get; set; }

    /// <summary>
    /// 配置实体模型映射
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUserEntity(modelBuilder);
        ConfigureMemberEntity(modelBuilder);
        ConfigureBoardGameEntity(modelBuilder);
        ConfigureTableEntity(modelBuilder);
        ConfigureOrderEntity(modelBuilder);
        ConfigureOrderItemEntity(modelBuilder);
        ConfigureOrderBoardGameEntity(modelBuilder);
        ConfigureReservationEntity(modelBuilder);
        ConfigureDamageRecordEntity(modelBuilder);
        ConfigureOperationLogEntity(modelBuilder);
        ConfigureRechargeRecordEntity(modelBuilder);
        ConfigureCouponEntity(modelBuilder);
        ConfigureBoardGameCopyEntity(modelBuilder);
        ConfigureRestockRecordEntity(modelBuilder);
        ConfigureMemberBalanceRecordEntity(modelBuilder);
        ConfigureSystemSettingEntity(modelBuilder);
    }

    /// <summary>
    /// 配置用户实体
    /// </summary>
    private static void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(u => u.Phone)
                .HasMaxLength(20);

            entity.Property(u => u.Email)
                .HasMaxLength(100);

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(u => u.IsDeleted)
                .HasDefaultValue(false);

            entity.HasIndex(u => u.Phone)
                .IsUnique()
                .HasFilter("[Phone] IS NOT NULL");

            entity.HasQueryFilter(u => !u.IsDeleted);
        });
    }

    /// <summary>
    /// 配置会员实体
    /// </summary>
    private static void ConfigureMemberEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.MemberNo)
                .HasMaxLength(50);

            entity.Property(m => m.UserId)
                .IsRequired();

            entity.Property(m => m.NickName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.Level)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(m => m.Points)
                .HasDefaultValue(0);

            entity.Property(m => m.Balance)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(m => m.Avatar)
                .HasMaxLength(500);

            entity.Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(m => m.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(m => m.UserId)
                .IsUnique();

            entity.HasOne<User>()
                .WithOne()
                .HasForeignKey<Member>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置桌游实体
    /// </summary>
    private static void ConfigureBoardGameEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.GameNo)
                .HasMaxLength(50);

            entity.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(b => b.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(b => b.MinPlayers)
                .IsRequired();

            entity.Property(b => b.MaxPlayers)
                .IsRequired();

            entity.Property(b => b.Description)
                .HasMaxLength(1000);

            entity.Property(b => b.ImageUrl)
                .HasMaxLength(500);

            entity.Property(b => b.TotalStock)
                .HasDefaultValue(0);

            entity.Property(b => b.CurrentStock)
                .HasDefaultValue(0);

            entity.Property(b => b.DamagedStock)
                .HasDefaultValue(0);

            entity.Property(b => b.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(b => b.PricePerHour)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(b => b.Name);

            entity.HasQueryFilter(b => !b.IsDeleted);
        });
    }

    /// <summary>
    /// 配置桌位实体
    /// </summary>
    private static void ConfigureTableEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(t => t.Area)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(t => t.Seats)
                .IsRequired();

            entity.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(t => t.HourlyRate)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(t => t.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(t => t.Code)
                .IsUnique();

            entity.HasIndex(t => t.Status);
        });
    }

    /// <summary>
    /// 配置订单实体
    /// </summary>
    private static void ConfigureOrderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderNo)
                .HasMaxLength(50);

            entity.Property(o => o.MemberId);

            entity.Property(o => o.TableId);

            entity.Property(o => o.StartTime)
                .IsRequired();

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(o => o.DiscountRate)
                .HasPrecision(18, 2)
                .HasDefaultValue(10m);

            entity.Property(o => o.Discount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(o => o.ActualAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(o => o.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(o => o.CreatedBy)
                .IsRequired();

            entity.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(o => o.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(o => o.MemberId);
            entity.HasIndex(o => o.TableId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CreatedAt);

            entity.HasOne<Member>()
                .WithMany()
                .HasForeignKey(o => o.MemberId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<Table>()
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// 配置订单项实体
    /// </summary>
    private static void ConfigureOrderItemEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.OrderId)
                .IsRequired();

            entity.Property(oi => oi.BoardGameId);

            entity.Property(oi => oi.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(oi => oi.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(oi => oi.Subtotal)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(oi => oi.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(oi => oi.OrderId);
            entity.HasIndex(oi => oi.BoardGameId);

            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(oi => oi.BoardGameId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// 配置订单-桌游关联实体
    /// </summary>
    private static void ConfigureOrderBoardGameEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderBoardGame>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderId)
                .IsRequired();

            entity.Property(o => o.BoardGameId)
                .IsRequired();

            entity.Property(o => o.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(o => o.IsReturned)
                .HasDefaultValue(false);

            entity.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(o => o.OrderId);
            entity.HasIndex(o => o.BoardGameId);

            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(o => o.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置预约实体
    /// </summary>
    private static void ConfigureReservationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.ReservationNo)
                .HasMaxLength(50);

            entity.Property(r => r.MemberId)
                .IsRequired();

            entity.Property(r => r.TableId);

            entity.Property(r => r.ReservedGames)
                .HasMaxLength(500);

            entity.Property(r => r.TableType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(r => r.Date)
                .IsRequired();

            entity.Property(r => r.StartTime)
                .IsRequired();

            entity.Property(r => r.EndTime)
                .IsRequired();

            entity.Property(r => r.GuestCount)
                .IsRequired();

            entity.Property(r => r.Deposit)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(r => r.ContactPhone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(r => r.Remark)
                .HasMaxLength(500);

            entity.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(r => r.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(r => r.MemberId);
            entity.HasIndex(r => r.TableId);
            entity.HasIndex(r => r.Date);
            entity.HasIndex(r => r.Status);

            entity.HasOne<Member>()
                .WithMany()
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Table>()
                .WithMany()
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// 配置损坏记录实体
    /// </summary>
    private static void ConfigureDamageRecordEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DamageRecord>(entity =>
        {
            entity.HasKey(dr => dr.Id);

            entity.Property(dr => dr.BoardGameId)
                .IsRequired();

            entity.Property(dr => dr.OrderId);

            entity.Property(dr => dr.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(dr => dr.Reason)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(dr => dr.Compensation)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(dr => dr.CreatedBy)
                .IsRequired();

            entity.Property(dr => dr.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(dr => dr.BoardGameId);
            entity.HasIndex(dr => dr.OrderId);

            entity.HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(dr => dr.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(dr => dr.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// 配置操作日志实体
    /// </summary>
    private static void ConfigureOperationLogEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OperationLog>(entity =>
        {
            entity.HasKey(ol => ol.Id);

            entity.Property(ol => ol.UserId);

            entity.Property(ol => ol.UserName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(ol => ol.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(ol => ol.TargetId)
                .HasMaxLength(100);

            entity.Property(ol => ol.Description)
                .HasMaxLength(1000);

            entity.Property(ol => ol.Timestamp)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(ol => ol.UserId);
            entity.HasIndex(ol => ol.Action);
            entity.HasIndex(ol => ol.Timestamp);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(ol => ol.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// 配置充值记录实体
    /// </summary>
    private static void ConfigureRechargeRecordEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RechargeRecord>(entity =>
        {
            entity.HasKey(rr => rr.Id);

            entity.Property(rr => rr.MemberId)
                .IsRequired();

            entity.Property(rr => rr.Amount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(rr => rr.Bonus)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(rr => rr.PayMethod)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(rr => rr.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(rr => rr.MemberId);
            entity.HasIndex(rr => rr.CreatedAt);

            entity.HasOne<Member>()
                .WithMany()
                .HasForeignKey(rr => rr.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置优惠券实体
    /// </summary>
    private static void ConfigureCouponEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.MemberId)
                .IsRequired();

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Discount)
                .HasPrecision(18, 2);

            entity.Property(c => c.DiscountAmount)
                .HasPrecision(18, 2);

            entity.Property(c => c.MinAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(c => c.ExpireDate)
                .IsRequired();

            entity.Property(c => c.IsUsed)
                .HasDefaultValue(false);

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(c => c.MemberId);
            entity.HasIndex(c => c.IsUsed);
            entity.HasIndex(c => c.ExpireDate);

            entity.HasOne<Member>()
                .WithMany()
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(c => c.UsedOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// 配置桌游副本实体
    /// </summary>
    private static void ConfigureBoardGameCopyEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardGameCopy>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.CopyNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(c => c.BoardGameId);
            entity.HasIndex(c => c.CopyNumber)
                .IsUnique();

            entity.HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(c => c.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置补货记录实体
    /// </summary>
    private static void ConfigureRestockRecordEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestockRecord>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Quantity)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(r => r.Remark)
                .HasMaxLength(500);

            entity.Property(r => r.CreatedBy)
                .IsRequired();

            entity.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(r => r.BoardGameId);
            entity.HasIndex(r => r.CreatedAt);

            entity.HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(r => r.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置会员余额流水实体
    /// </summary>
    private static void ConfigureMemberBalanceRecordEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemberBalanceRecord>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Type)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(r => r.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(r => r.Description)
                .HasMaxLength(500);

            entity.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(r => r.MemberId);
            entity.HasIndex(r => r.CreatedAt);
        });
    }

    /// <summary>
    /// 配置系统设置实体（Key 唯一索引）
    /// </summary>
    private static void ConfigureSystemSettingEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.Value)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(s => s.Description)
                .HasMaxLength(200);

            entity.HasIndex(s => s.Key)
                .IsUnique();

            entity.Property(s => s.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");
        });
    }
}
