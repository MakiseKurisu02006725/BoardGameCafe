using BoardGameCafe.Common.Helpers;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;
using BoardGameCafe.Core.Interfaces;
using BoardGameCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoardGameCafe.API.Data;

/// <summary>
/// 数据库初始化器，用于创建数据库和种子数据
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// 会员等级折扣配置
    /// </summary>
    public static readonly Dictionary<MemberLevel, decimal> MemberLevelDiscounts = new()
    {
        { MemberLevel.Bronze, 1.0m },
        { MemberLevel.Silver, 0.9m },
        { MemberLevel.Gold, 0.8m },
        { MemberLevel.Diamond, 0.7m }
    };

    /// <summary>
    /// 初始化数据库，确保数据库已创建并填充种子数据
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>任务</returns>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        try
        {
            var context = serviceProvider.GetRequiredService<BoardGameCafeDbContext>();

            await context.Database.EnsureCreatedAsync();

            // 针对已存在的数据库补充业务编号列（EnsureCreatedAsync 不会为已存在数据库新增列）
            EnsureBusinessNoColumns(context);

            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

            SeedUsers(unitOfWork);
            SeedTables(unitOfWork);
            SeedBoardGames(unitOfWork);

            // 为历史数据补齐业务编号（幂等：只填充空编号的记录）
            BackfillBusinessNos(unitOfWork);

            // 为存量桌游初始化副本记录（幂等：仅处理无副本记录的桌游）
            InitializeCopyRecords(unitOfWork);

            // 校正桌位状态：无进行中预约的"已预约"桌位恢复为空闲（修复预约完成/取消后桌位状态未同步导致的卡住）
            CorrectStuckTableStatuses(unitOfWork);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("数据库初始化失败", ex);
        }
    }

    /// <summary>
    /// 确保业务编号列存在（针对旧数据库通过原生SQL补列）
    /// </summary>
    /// <param name="context">数据库上下文</param>
    private static void EnsureBusinessNoColumns(BoardGameCafeDbContext context)
    {
        var columns = new (string Table, string Column)[]
        {
            ("Members", "MemberNo"),
            ("Orders", "OrderNo"),
            ("BoardGames", "GameNo"),
            ("Reservations", "ReservationNo")
        };

        foreach (var (table, column) in columns)
        {
            var sql = $@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}')
BEGIN
    ALTER TABLE [{table}] ADD [{column}] NVARCHAR(50) NOT NULL DEFAULT ('');
END";
            context.Database.ExecuteSqlRaw(sql);
        }

        // 旧库 DamageRecords.Compensation 列可能缺少默认值约束：
        // 实体配置了 HasDefaultValue(0)，EF 插入时跳过该列依赖数据库默认值，缺约束会导致 NULL 插入失败
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('DamageRecords')
      AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('DamageRecords'), 'Compensation', 'ColumnId')
)
AND EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DamageRecords' AND COLUMN_NAME = 'Compensation'
)
BEGIN
    ALTER TABLE [DamageRecords] ADD CONSTRAINT DF_DamageRecords_Compensation DEFAULT (0) FOR [Compensation];
END");

        // 旧库 Orders 表补充折扣折数列（默认 10 折 = 原价）
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'DiscountRate')
BEGIN
    ALTER TABLE [Orders] ADD [DiscountRate] decimal(18,2) NOT NULL DEFAULT (10);
END");

        // 旧库 Reservations 表补充 ReservedGames 列（预约桌游名称）
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reservations' AND COLUMN_NAME = 'ReservedGames')
BEGIN
    ALTER TABLE [Reservations] ADD [ReservedGames] NVARCHAR(500) NULL;
END");

        // 旧库不会自动创建新增的表：桌游副本表
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'BoardGameCopies' AND xtype = 'U')
BEGIN
    CREATE TABLE [BoardGameCopies] (
        [Id] uniqueidentifier NOT NULL,
        [BoardGameId] uniqueidentifier NOT NULL,
        [CopyNumber] nvarchar(50) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_BoardGameCopies] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_BoardGameCopies_BoardGameId] ON [BoardGameCopies] ([BoardGameId]);
    CREATE UNIQUE INDEX [IX_BoardGameCopies_CopyNumber] ON [BoardGameCopies] ([CopyNumber]);
END");

        // 补货记录表
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'RestockRecords' AND xtype = 'U')
BEGIN
    CREATE TABLE [RestockRecords] (
        [Id] uniqueidentifier NOT NULL,
        [BoardGameId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL DEFAULT (0),
        [Remark] nvarchar(500) NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_RestockRecords] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_RestockRecords_BoardGameId] ON [RestockRecords] ([BoardGameId]);
    CREATE INDEX [IX_RestockRecords_CreatedAt] ON [RestockRecords] ([CreatedAt]);
END");

        // 统计查询索引：热门桌游按 BoardGameId 分组、订单按 Status 筛选
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_BoardGameId' AND object_id = OBJECT_ID('OrderItems'))
BEGIN
    CREATE INDEX [IX_OrderItems_BoardGameId] ON [OrderItems] ([BoardGameId]);
END");

        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
END");

        // 预约-桌游联动列：预约存储桌游Id列表、副本关联预约、订单关联预约
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reservations' AND COLUMN_NAME = 'ReservedGameIds')
BEGIN
    ALTER TABLE [Reservations] ADD [ReservedGameIds] NVARCHAR(500) NULL;
END");

        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BoardGameCopies' AND COLUMN_NAME = 'ReservationId')
BEGIN
    ALTER TABLE [BoardGameCopies] ADD [ReservationId] uniqueidentifier NULL;
END");

        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BoardGameCopies' AND COLUMN_NAME = 'Source')
BEGIN
    ALTER TABLE [BoardGameCopies] ADD [Source] int NOT NULL DEFAULT (1);
END");

        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'ReservationId')
BEGIN
    ALTER TABLE [Orders] ADD [ReservationId] uniqueidentifier NULL;
END");

        // 系统配置表：日志清理等运行时配置（键值对）
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'SystemSettings' AND xtype = 'U')
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(500) NOT NULL,
        [Description] nvarchar(200) NULL,
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [SystemSettings] ([Key]);
END");

        // 会员余额流水表：记录余额变动（充值/消费扣款），用于核对余额构成
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'MemberBalanceRecords' AND xtype = 'U')
BEGIN
    CREATE TABLE [MemberBalanceRecords] (
        [Id] uniqueidentifier NOT NULL,
        [MemberId] uniqueidentifier NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [OrderId] uniqueidentifier NULL,
        [Description] nvarchar(500) NOT NULL DEFAULT (''),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_MemberBalanceRecords] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_MemberBalanceRecords_MemberId] ON [MemberBalanceRecords] ([MemberId]);
    CREATE INDEX [IX_MemberBalanceRecords_CreatedAt] ON [MemberBalanceRecords] ([CreatedAt]);
END");

        // 订单-桌游关联表：开台选择的游玩桌游（结账/退款后归还库存）
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'OrderBoardGames' AND xtype = 'U')
BEGIN
    CREATE TABLE [OrderBoardGames] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [BoardGameId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL DEFAULT (1),
        [IsReturned] bit NOT NULL DEFAULT (0),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_OrderBoardGames] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_OrderBoardGames_OrderId] ON [OrderBoardGames] ([OrderId]);
    CREATE INDEX [IX_OrderBoardGames_BoardGameId] ON [OrderBoardGames] ([BoardGameId]);
END");

        // 种子默认清理配置（幂等：仅插入不存在的键）
        context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = 'LogCleanup:QueryDays')
    INSERT INTO SystemSettings (Id, [Key], [Value], [Description]) VALUES (NEWID(), 'LogCleanup:QueryDays', '30', '查询日志（QUERY）保留天数');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = 'LogCleanup:OperationDays')
    INSERT INTO SystemSettings (Id, [Key], [Value], [Description]) VALUES (NEWID(), 'LogCleanup:OperationDays', '90', '操作日志（CREATE/UPDATE/DELETE）保留天数');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = 'LogCleanup:LoginDays')
    INSERT INTO SystemSettings (Id, [Key], [Value], [Description]) VALUES (NEWID(), 'LogCleanup:LoginDays', '180', '登录登出日志（LOGIN/LOGOUT）保留天数');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = 'LogCleanup:Enabled')
    INSERT INTO SystemSettings (Id, [Key], [Value], [Description]) VALUES (NEWID(), 'LogCleanup:Enabled', 'true', '是否启用每日自动清理');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = 'LogCleanup:ExecuteTime')
    INSERT INTO SystemSettings (Id, [Key], [Value], [Description]) VALUES (NEWID(), 'LogCleanup:ExecuteTime', '02:00', '每日执行清理时间（HH:mm）');
");
    }

    /// <summary>
    /// 为历史数据补齐业务编号（幂等，只填充空编号的记录）
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    private static void BackfillBusinessNos(IUnitOfWork unitOfWork)
    {
        // 会员号：M + 序号（M1、M2...）
        var members = unitOfWork.Members.GetAll().OrderBy(m => m.CreatedAt).ToList();
        var memberSeq = members
            .Where(m => !string.IsNullOrEmpty(m.MemberNo) && m.MemberNo.StartsWith("M") && m.MemberNo.Length > 1)
            .Select(m => int.TryParse(m.MemberNo[1..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
        foreach (var member in members)
        {
            if (!string.IsNullOrEmpty(member.MemberNo)) continue;
            memberSeq++;
            member.MemberNo = $"M{memberSeq}";
            unitOfWork.Members.Update(member);
        }

        // 桌游编号：B + 4位序号（B0001、B0002...）
        var games = unitOfWork.BoardGames.GetAll().OrderBy(g => g.CreatedAt).ToList();
        var gameSeq = games
            .Where(g => !string.IsNullOrEmpty(g.GameNo) && g.GameNo.StartsWith("B") && g.GameNo.Length > 1)
            .Select(g => int.TryParse(g.GameNo[1..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
        foreach (var game in games)
        {
            if (!string.IsNullOrEmpty(game.GameNo)) continue;
            gameSeq++;
            game.GameNo = $"B{gameSeq:D4}";
            unitOfWork.BoardGames.Update(game);
        }

        // 预约号：RESV + 3位序号（RESV001、RESV002...）
        var reservations = unitOfWork.Reservations.GetAll().OrderBy(r => r.CreatedAt).ToList();
        var resSeq = reservations
            .Where(r => !string.IsNullOrEmpty(r.ReservationNo) && r.ReservationNo.StartsWith("RESV") && r.ReservationNo.Length > 4)
            .Select(r => int.TryParse(r.ReservationNo[4..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
        foreach (var reservation in reservations)
        {
            if (!string.IsNullOrEmpty(reservation.ReservationNo)) continue;
            resSeq++;
            reservation.ReservationNo = $"RESV{resSeq:D3}";
            unitOfWork.Reservations.Update(reservation);
        }

        // 订单号：ORD + 日期 + 3位当日序号（ORD202408100001...）
        var orders = unitOfWork.Orders.GetAll().OrderBy(o => o.CreatedAt).ToList();
        foreach (var dayGroup in orders.GroupBy(o => o.CreatedAt.Date))
        {
            var orderSeq = 0;
            foreach (var order in dayGroup)
            {
                if (!string.IsNullOrEmpty(order.OrderNo)) continue;
                orderSeq++;
                order.OrderNo = $"ORD{dayGroup.Key:yyyyMMdd}{orderSeq:D3}";
                unitOfWork.Orders.Update(order);
            }
        }

        unitOfWork.SaveChanges();
    }

    /// <summary>
    /// 为存量桌游初始化副本记录（幂等：仅处理无副本记录的桌游）
    /// 保证借出编号自动生成规则（拼音首字母+序号）对历史库存同样生效
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    private static void InitializeCopyRecords(IUnitOfWork unitOfWork)
    {
        var allCopyNos = unitOfWork.BoardGameCopies.GetAll().Select(c => c.CopyNumber).ToList();
        var games = unitOfWork.BoardGames.GetAll().Where(g => !g.IsDeleted).ToList();
        var now = DateTime.Now;

        foreach (var game in games)
        {
            var hasCopies = unitOfWork.BoardGameCopies.Find(c => c.BoardGameId == game.Id).Any();
            if (hasCopies)
            {
                continue;
            }

            var prefix = PinyinHelper.GetInitials(game.Name);
            var seq = ParseCopyMaxSeq(allCopyNos, prefix) + 1;

            // 在库副本
            for (var i = 0; i < game.CurrentStock; i++)
            {
                var copyNo = $"{prefix}{seq++:D3}";
                allCopyNos.Add(copyNo);
                unitOfWork.BoardGameCopies.Add(new BoardGameCopy
                {
                    Id = Guid.NewGuid(),
                    BoardGameId = game.Id,
                    CopyNumber = copyNo,
                    Status = CopyStatus.InStock,
                    CreatedAt = now
                });
            }

            // 损坏副本
            for (var i = 0; i < game.DamagedStock; i++)
            {
                var copyNo = $"{prefix}{seq++:D3}";
                allCopyNos.Add(copyNo);
                unitOfWork.BoardGameCopies.Add(new BoardGameCopy
                {
                    Id = Guid.NewGuid(),
                    BoardGameId = game.Id,
                    CopyNumber = copyNo,
                    Status = CopyStatus.Damaged,
                    CreatedAt = now
                });
            }
        }

        unitOfWork.SaveChanges();
    }

    /// <summary>
    /// 解析指定前缀副本编号中的最大数字序号（如 CG001 → 1）
    /// </summary>
    /// <param name="copyNos">副本编号集合</param>
    /// <param name="prefix">编号前缀</param>
    /// <returns>最大序号（无匹配时为0）</returns>
    private static int ParseCopyMaxSeq(IEnumerable<string> copyNos, string prefix)
    {
        return copyNos
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && n.Length > prefix.Length)
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
    }

    /// <summary>
    /// 校正桌位状态：将标记为"已预约"但没有进行中预约（待确认/已确认）的桌位恢复为空闲。
    /// 解决预约完成/取消后桌位状态未同步导致桌位卡在"已预约"的问题。幂等，每次启动执行。
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    private static void CorrectStuckTableStatuses(IUnitOfWork unitOfWork)
    {
        var reservedTables = unitOfWork.Tables
            .Find(t => t.Status == TableStatus.Reserved)
            .ToList();
        if (reservedTables.Count == 0)
        {
            return;
        }

        var tableIds = reservedTables.Select(t => t.Id).ToList();
        var activeReservationTableIds = unitOfWork.Reservations
            .Find(r => r.TableId.HasValue &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                       tableIds.Contains(r.TableId ?? Guid.Empty))
            .Select(r => r.TableId ?? Guid.Empty)
            .ToHashSet();

        var now = DateTime.Now;
        var corrected = 0;
        foreach (var table in reservedTables.Where(t => !activeReservationTableIds.Contains(t.Id)))
        {
            table.Status = TableStatus.Idle;
            table.UpdatedAt = now;
            unitOfWork.Tables.Update(table);
            corrected++;
        }

        if (corrected > 0)
        {
            unitOfWork.SaveChanges();
        }
    }

    /// <summary>
    /// 种子用户数据（管理员、店员账号）
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    private static void SeedUsers(IUnitOfWork unitOfWork)
    {
        if (unitOfWork.Users.GetAll().Any())
        {
            return;
        }

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                Role = UserRole.Admin,
                Phone = "13800000000",
                Email = "admin@boardgamecafe.com",
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "manager",
                PasswordHash = PasswordHelper.HashPassword("manager123"),
                Role = UserRole.Manager,
                Phone = "13800000001",
                Email = "manager@boardgamecafe.com",
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "cashier",
                PasswordHash = PasswordHelper.HashPassword("cashier123"),
                Role = UserRole.Cashier,
                Phone = "13800000002",
                Email = "cashier@boardgamecafe.com",
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "staff",
                PasswordHash = PasswordHelper.HashPassword("staff123"),
                Role = UserRole.Cashier,
                Phone = "13800000003",
                Email = "staff@boardgamecafe.com",
                CreatedAt = DateTime.Now
            }
        };

        foreach (var user in users)
        {
            unitOfWork.Users.Add(user);
        }

        unitOfWork.SaveChanges();
    }

    /// <summary>
    /// 种子桌位数据（大厅6桌+卡座4+包间2）
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    private static void SeedTables(IUnitOfWork unitOfWork)
    {
        if (unitOfWork.Tables.GetAll().Any())
        {
            return;
        }

        var tables = new List<Table>();

        for (int i = 1; i <= 6; i++)
        {
            tables.Add(new Table
            {
                Id = Guid.NewGuid(),
                Code = $"H{i:D2}",
                Area = TableArea.Hall,
                Seats = 4 + (i % 3) * 2,
                Status = TableStatus.Idle,
                HourlyRate = 20,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        for (int i = 1; i <= 4; i++)
        {
            tables.Add(new Table
            {
                Id = Guid.NewGuid(),
                Code = $"B{i:D2}",
                Area = TableArea.Booth,
                Seats = 4,
                Status = TableStatus.Idle,
                HourlyRate = 25,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        for (int i = 1; i <= 2; i++)
        {
            tables.Add(new Table
            {
                Id = Guid.NewGuid(),
                Code = $"P{i:D2}",
                Area = TableArea.PrivateRoom,
                Seats = 8,
                Status = TableStatus.Idle,
                HourlyRate = 50,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        foreach (var table in tables)
        {
            unitOfWork.Tables.Add(table);
        }

        unitOfWork.SaveChanges();
    }

    /// <summary>
    /// 种子桌游数据（5-10种）
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    private static void SeedBoardGames(IUnitOfWork unitOfWork)
    {
        if (unitOfWork.BoardGames.GetAll().Any())
        {
            return;
        }

        var boardGames = new List<BoardGame>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "卡坦岛",
                Category = GameCategory.Strategy,
                MinPlayers = 3,
                MaxPlayers = 4,
                Description = "经典策略桌游，玩家通过建设定居点、城市和道路来扩张领土。",
                TotalStock = 3,
                CurrentStock = 3,
                DamagedStock = 0,
                PricePerHour = 15,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "狼人杀",
                Category = GameCategory.Party,
                MinPlayers = 6,
                MaxPlayers = 18,
                Description = "经典多人社交推理游戏，适合派对和团建。",
                TotalStock = 5,
                CurrentStock = 5,
                DamagedStock = 0,
                PricePerHour = 10,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "大富翁",
                Category = GameCategory.Family,
                MinPlayers = 2,
                MaxPlayers = 6,
                Description = "经典家庭桌游，通过购买地产、收取租金成为最富有的玩家。",
                TotalStock = 4,
                CurrentStock = 4,
                DamagedStock = 0,
                PricePerHour = 12,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "UNO",
                Category = GameCategory.CardGame,
                MinPlayers = 2,
                MaxPlayers = 10,
                Description = "经典卡牌游戏，规则简单有趣，适合各年龄段。",
                TotalStock = 8,
                CurrentStock = 8,
                DamagedStock = 0,
                PricePerHour = 8,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "璀璨宝石",
                Category = GameCategory.Strategy,
                MinPlayers = 2,
                MaxPlayers = 4,
                Description = "收集宝石、购买卡牌、争夺贵族特权的策略游戏。",
                TotalStock = 3,
                CurrentStock = 3,
                DamagedStock = 0,
                PricePerHour = 15,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "瘟疫危机",
                Category = GameCategory.Cooperation,
                MinPlayers = 2,
                MaxPlayers = 4,
                Description = "合作类桌游，玩家共同对抗全球瘟疫蔓延。",
                TotalStock = 2,
                CurrentStock = 2,
                DamagedStock = 0,
                PricePerHour = 18,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "达芬奇密码",
                Category = GameCategory.Family,
                MinPlayers = 2,
                MaxPlayers = 4,
                Description = "数字推理类桌游，通过逻辑推理猜出对方的数字密码。",
                TotalStock = 4,
                CurrentStock = 4,
                DamagedStock = 0,
                PricePerHour = 10,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "三国杀",
                Category = GameCategory.CardGame,
                MinPlayers = 5,
                MaxPlayers = 10,
                Description = "经典国风卡牌游戏，以三国为背景的身份推理游戏。",
                TotalStock = 5,
                CurrentStock = 5,
                DamagedStock = 0,
                PricePerHour = 12,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "种田记",
                Category = GameCategory.Strategy,
                MinPlayers = 1,
                MaxPlayers = 5,
                Description = "资源管理类策略游戏，玩家经营农场、积累财富。",
                TotalStock = 2,
                CurrentStock = 2,
                DamagedStock = 0,
                PricePerHour = 16,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "猜猜画画",
                Category = GameCategory.Party,
                MinPlayers = 3,
                MaxPlayers = 8,
                Description = "你画我猜类派对游戏，适合朋友聚会娱乐。",
                TotalStock = 6,
                CurrentStock = 6,
                DamagedStock = 0,
                PricePerHour = 8,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        foreach (var game in boardGames)
        {
            unitOfWork.BoardGames.Add(game);
        }

        unitOfWork.SaveChanges();
    }
}
