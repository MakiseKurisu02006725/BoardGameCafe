-- =====================================================
-- 数据库：BoardGameCafe 桌游吧管理系统
-- 数据库版本：SQL Server 2022
-- 说明：该脚本仅创建数据库结构，不包含种子数据
--       种子数据由 DbInitializer 动态生成（尤其密码需通过 HashPassword 动态生成）
-- =====================================================

-- 1. 创建数据库（如果不存在）
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BoardGameCafe')
BEGIN
    CREATE DATABASE [BoardGameCafe]
    ON PRIMARY
    (
        NAME = N'BoardGameCafe_Data',
        FILENAME = N'D:\SQLServer\BoardGameCafe_Data.mdf',
        SIZE = 10MB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 10%
    )
    LOG ON
    (
        NAME = N'BoardGameCafe_Log',
        FILENAME = N'D:\SQLServer\BoardGameCafe_Log.ldf',
        SIZE = 5MB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 10%
    );
END
GO

-- 2. 切换到目标数据库
USE [BoardGameCafe];
GO

-- =====================================================
-- 3. 创建表（按依赖顺序）
-- =====================================================

-- -----------------------------------------------------
-- 表：Users 用户表
-- 说明：存储系统登录用户（管理员、店员等）
-- 依赖：无
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Users')
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id]          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [UserName]    NVARCHAR(50)     NOT NULL,
        [PasswordHash] NVARCHAR(200)   NOT NULL,
        [Role]        NVARCHAR(20)     NOT NULL,
        [Phone]       NVARCHAR(20)     NOT NULL,
        [Email]       NVARCHAR(100)        NULL,
        [CreatedAt]   DATETIME2        NOT NULL DEFAULT GETDATE(),
        [IsDeleted]   BIT              NOT NULL DEFAULT 0
    );
    PRINT 'Users 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：Members 会员表
-- 说明：存储会员信息，与 Users 为 1:0..1 关系
-- 依赖：Users 表
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Members')
BEGIN
    CREATE TABLE [dbo].[Members] (
        [Id]         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [UserId]     UNIQUEIDENTIFIER     NULL UNIQUE,
        [NickName]   NVARCHAR(50)     NOT NULL,
        [Level]      NVARCHAR(20)     NOT NULL,
        [Points]     INT              NOT NULL DEFAULT 0,
        [Balance]    DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [Birthday]   DATE                 NULL,
        [Avatar]     NVARCHAR(200)        NULL,
        [CreatedAt]  DATETIME2        NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]  DATETIME2            NULL
    );
    PRINT 'Members 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：BoardGames 桌游表
-- 说明：存储桌游商品信息
-- 依赖：无
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'BoardGames')
BEGIN
    CREATE TABLE [dbo].[BoardGames] (
        [Id]            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name]          NVARCHAR(100)    NOT NULL,
        [Category]      NVARCHAR(20)     NOT NULL,
        [MinPlayers]    INT              NOT NULL,
        [MaxPlayers]    INT              NOT NULL,
        [Description]   NVARCHAR(1000)       NULL,
        [ImageUrl]      NVARCHAR(500)        NULL,
        [TotalStock]    INT              NOT NULL DEFAULT 0,
        [CurrentStock]  INT              NOT NULL DEFAULT 0,
        [DamagedStock]  INT              NOT NULL DEFAULT 0,
        [IsDeleted]     BIT              NOT NULL DEFAULT 0,
        [PricePerHour]  DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [CreatedAt]     DATETIME2        NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]     DATETIME2            NULL
    );
    PRINT 'BoardGames 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：Tables 桌台表
-- 说明：存储店内桌台信息
-- 依赖：无
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Tables')
BEGIN
    CREATE TABLE [dbo].[Tables] (
        [Id]         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Code]       NVARCHAR(20)     NOT NULL UNIQUE,
        [Area]       NVARCHAR(20)     NOT NULL,
        [Seats]      INT              NOT NULL,
        [Status]     NVARCHAR(20)     NOT NULL,
        [HourlyRate] DECIMAL(18,2)    NOT NULL,
        [CreatedAt]  DATETIME2        NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]  DATETIME2            NULL
    );
    PRINT 'Tables 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：Orders 订单表
-- 说明：存储消费订单主表
-- 依赖：Members, Tables
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Orders')
BEGIN
    CREATE TABLE [dbo].[Orders] (
        [Id]            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MemberId]      UNIQUEIDENTIFIER     NULL,
        [TableId]       UNIQUEIDENTIFIER     NULL,
        [StartTime]     DATETIME2            NULL,
        [EndTime]       DATETIME2            NULL,
        [TotalAmount]   DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [Discount]      DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [ActualAmount]  DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [Status]        NVARCHAR(20)     NOT NULL,
        [PaymentMethod] NVARCHAR(20)         NULL,
        [CreatedBy]     UNIQUEIDENTIFIER     NULL,
        [PaidAt]        DATETIME2            NULL,
        [CreatedAt]     DATETIME2        NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]     DATETIME2            NULL
    );
    PRINT 'Orders 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：OrderItems 订单明细表
-- 说明：存储订单的明细项目（桌台费用、桌游费用等）
-- 依赖：Orders, BoardGames
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'OrderItems')
BEGIN
    CREATE TABLE [dbo].[OrderItems] (
        [Id]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [OrderId]      UNIQUEIDENTIFIER NOT NULL,
        [BoardGameId]  UNIQUEIDENTIFIER     NULL,
        [Name]         NVARCHAR(100)    NOT NULL,
        [Quantity]     INT              NOT NULL,
        [UnitPrice]    DECIMAL(18,2)    NOT NULL,
        [Type]         NVARCHAR(20)     NOT NULL,
        [Subtotal]     DECIMAL(18,2)    NOT NULL,
        [CreatedAt]    DATETIME2        NOT NULL DEFAULT GETDATE()
    );
    PRINT 'OrderItems 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：Reservations 预约表
-- 说明：存储会员预约信息
-- 依赖：Members, Tables
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Reservations')
BEGIN
    CREATE TABLE [dbo].[Reservations] (
        [Id]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MemberId]     UNIQUEIDENTIFIER NOT NULL,
        [TableId]      UNIQUEIDENTIFIER     NULL,
        [TableType]    NVARCHAR(20)     NOT NULL,
        [Date]         DATE             NOT NULL,
        [StartTime]    TIME             NOT NULL,
        [EndTime]      TIME             NOT NULL,
        [GuestCount]   INT              NOT NULL,
        [Deposit]      DECIMAL(18,2)    NOT NULL,
        [Status]       NVARCHAR(20)     NOT NULL,
        [ContactPhone] NVARCHAR(20)     NOT NULL,
        [Remark]       NVARCHAR(500)        NULL,
        [CreatedAt]    DATETIME2        NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]    DATETIME2            NULL
    );
    PRINT 'Reservations 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：DamageRecords 损坏记录表
-- 说明：存储桌游损坏赔偿记录
-- 依赖：BoardGames, Orders
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'DamageRecords')
BEGIN
    CREATE TABLE [dbo].[DamageRecords] (
        [Id]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [BoardGameId]  UNIQUEIDENTIFIER NOT NULL,
        [OrderId]      UNIQUEIDENTIFIER     NULL,
        [Quantity]     INT              NOT NULL,
        [Reason]       NVARCHAR(500)    NOT NULL,
        [Compensation] DECIMAL(18,2)    NOT NULL,
        [CreatedBy]    UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt]    DATETIME2        NOT NULL DEFAULT GETDATE()
    );
    PRINT 'DamageRecords 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：OperationLogs 操作日志表
-- 说明：存储系统操作日志
-- 依赖：无（UserId 为软关联，不设物理外键以便保留日志）
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'OperationLogs')
BEGIN
    CREATE TABLE [dbo].[OperationLogs] (
        [Id]          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [UserId]      UNIQUEIDENTIFIER     NULL,
        [UserName]    NVARCHAR(50)         NULL,
        [Action]      NVARCHAR(100)    NOT NULL,
        [TargetId]    NVARCHAR(100)        NULL,
        [Description] NVARCHAR(1000)       NULL,
        [Timestamp]   DATETIME2        NOT NULL DEFAULT GETDATE()
    );
    PRINT 'OperationLogs 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：RechargeRecords 充值记录表
-- 说明：存储会员充值记录
-- 依赖：Members
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'RechargeRecords')
BEGIN
    CREATE TABLE [dbo].[RechargeRecords] (
        [Id]        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MemberId]  UNIQUEIDENTIFIER NOT NULL,
        [Amount]    DECIMAL(18,2)    NOT NULL,
        [Bonus]     DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [PayMethod] NVARCHAR(20)     NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER     NULL,
        [CreatedAt] DATETIME2        NOT NULL DEFAULT GETDATE()
    );
    PRINT 'RechargeRecords 表创建成功';
END
GO

-- -----------------------------------------------------
-- 表：Coupons 优惠券表
-- 说明：存储会员优惠券信息
-- 依赖：Members, Orders
-- -----------------------------------------------------
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Coupons')
BEGIN
    CREATE TABLE [dbo].[Coupons] (
        [Id]             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MemberId]       UNIQUEIDENTIFIER NOT NULL,
        [Name]           NVARCHAR(100)    NOT NULL,
        [Discount]       DECIMAL(18,2)        NULL,
        [DiscountAmount] DECIMAL(18,2)        NULL,
        [MinAmount]      DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [ExpireDate]     DATETIME2        NOT NULL,
        [IsUsed]         BIT              NOT NULL DEFAULT 0,
        [UsedOrderId]    UNIQUEIDENTIFIER     NULL,
        [CreatedAt]      DATETIME2        NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Coupons 表创建成功';
END
GO

-- =====================================================
-- 4. 创建外键约束
-- =====================================================

-- Members -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Members_Users_UserId')
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD CONSTRAINT [FK_Members_Users_UserId]
        FOREIGN KEY ([UserId])
        REFERENCES [dbo].[Users] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_Members_Users_UserId 创建成功';
END
GO

-- Orders -> Members（删除会员时订单保留，会员置空）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Orders_Members_MemberId')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD CONSTRAINT [FK_Orders_Members_MemberId]
        FOREIGN KEY ([MemberId])
        REFERENCES [dbo].[Members] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_Orders_Members_MemberId 创建成功';
END
GO

-- Orders -> Tables（删除桌台时订单保留，桌台置空）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Orders_Tables_TableId')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD CONSTRAINT [FK_Orders_Tables_TableId]
        FOREIGN KEY ([TableId])
        REFERENCES [dbo].[Tables] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_Orders_Tables_TableId 创建成功';
END
GO

-- OrderItems -> Orders（删除订单时级联删除明细）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_OrderItems_Orders_OrderId')
BEGIN
    ALTER TABLE [dbo].[OrderItems]
    ADD CONSTRAINT [FK_OrderItems_Orders_OrderId]
        FOREIGN KEY ([OrderId])
        REFERENCES [dbo].[Orders] ([Id])
        ON DELETE CASCADE;
    PRINT '外键 FK_OrderItems_Orders_OrderId 创建成功';
END
GO

-- OrderItems -> BoardGames（删除桌游时订单明细保留，桌游置空）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_OrderItems_BoardGames_BoardGameId')
BEGIN
    ALTER TABLE [dbo].[OrderItems]
    ADD CONSTRAINT [FK_OrderItems_BoardGames_BoardGameId]
        FOREIGN KEY ([BoardGameId])
        REFERENCES [dbo].[BoardGames] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_OrderItems_BoardGames_BoardGameId 创建成功';
END
GO

-- Reservations -> Members（删除会员时级联删除预约）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Reservations_Members_MemberId')
BEGIN
    ALTER TABLE [dbo].[Reservations]
    ADD CONSTRAINT [FK_Reservations_Members_MemberId]
        FOREIGN KEY ([MemberId])
        REFERENCES [dbo].[Members] ([Id])
        ON DELETE CASCADE;
    PRINT '外键 FK_Reservations_Members_MemberId 创建成功';
END
GO

-- Reservations -> Tables（删除桌台时预约保留，桌台置空）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Reservations_Tables_TableId')
BEGIN
    ALTER TABLE [dbo].[Reservations]
    ADD CONSTRAINT [FK_Reservations_Tables_TableId]
        FOREIGN KEY ([TableId])
        REFERENCES [dbo].[Tables] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_Reservations_Tables_TableId 创建成功';
END
GO

-- DamageRecords -> BoardGames
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_DamageRecords_BoardGames_BoardGameId')
BEGIN
    ALTER TABLE [dbo].[DamageRecords]
    ADD CONSTRAINT [FK_DamageRecords_BoardGames_BoardGameId]
        FOREIGN KEY ([BoardGameId])
        REFERENCES [dbo].[BoardGames] ([Id]);
    PRINT '外键 FK_DamageRecords_BoardGames_BoardGameId 创建成功';
END
GO

-- DamageRecords -> Orders（删除订单时损坏记录保留，订单置空）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_DamageRecords_Orders_OrderId')
BEGIN
    ALTER TABLE [dbo].[DamageRecords]
    ADD CONSTRAINT [FK_DamageRecords_Orders_OrderId]
        FOREIGN KEY ([OrderId])
        REFERENCES [dbo].[Orders] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_DamageRecords_Orders_OrderId 创建成功';
END
GO

-- RechargeRecords -> Members（删除会员时级联删除充值记录）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_RechargeRecords_Members_MemberId')
BEGIN
    ALTER TABLE [dbo].[RechargeRecords]
    ADD CONSTRAINT [FK_RechargeRecords_Members_MemberId]
        FOREIGN KEY ([MemberId])
        REFERENCES [dbo].[Members] ([Id])
        ON DELETE CASCADE;
    PRINT '外键 FK_RechargeRecords_Members_MemberId 创建成功';
END
GO

-- Coupons -> Members（删除会员时级联删除优惠券）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Coupons_Members_MemberId')
BEGIN
    ALTER TABLE [dbo].[Coupons]
    ADD CONSTRAINT [FK_Coupons_Members_MemberId]
        FOREIGN KEY ([MemberId])
        REFERENCES [dbo].[Members] ([Id])
        ON DELETE CASCADE;
    PRINT '外键 FK_Coupons_Members_MemberId 创建成功';
END
GO

-- Coupons -> Orders（删除订单时优惠券保留，使用订单置空）
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Coupons_Orders_UsedOrderId')
BEGIN
    ALTER TABLE [dbo].[Coupons]
    ADD CONSTRAINT [FK_Coupons_Orders_UsedOrderId]
        FOREIGN KEY ([UsedOrderId])
        REFERENCES [dbo].[Orders] ([Id])
        ON DELETE SET NULL;
    PRINT '外键 FK_Coupons_Orders_UsedOrderId 创建成功';
END
GO

-- =====================================================
-- 5. 创建索引
-- =====================================================

-- Users 表索引
-- 手机号唯一索引（业务唯一键）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_Phone' AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Phone] ON [dbo].[Users] ([Phone] ASC);
    PRINT '索引 IX_Users_Phone 创建成功';
END
GO

-- 用户角色索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_Role' AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_Role] ON [dbo].[Users] ([Role] ASC);
    PRINT '索引 IX_Users_Role 创建成功';
END
GO

-- 用户创建时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_CreatedAt] ON [dbo].[Users] ([CreatedAt] DESC);
    PRINT '索引 IX_Users_CreatedAt 创建成功';
END
GO

-- Members 表索引
-- 外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Members_UserId' AND object_id = OBJECT_ID(N'dbo.Members'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Members_UserId] ON [dbo].[Members] ([UserId] ASC);
    PRINT '索引 IX_Members_UserId 创建成功';
END
GO

-- 会员等级索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Members_Level' AND object_id = OBJECT_ID(N'dbo.Members'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Members_Level] ON [dbo].[Members] ([Level] ASC);
    PRINT '索引 IX_Members_Level 创建成功';
END
GO

-- 会员创建时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Members_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Members'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Members_CreatedAt] ON [dbo].[Members] ([CreatedAt] DESC);
    PRINT '索引 IX_Members_CreatedAt 创建成功';
END
GO

-- 会员生日索引（用于生日营销）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Members_Birthday' AND object_id = OBJECT_ID(N'dbo.Members'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Members_Birthday] ON [dbo].[Members] ([Birthday] ASC);
    PRINT '索引 IX_Members_Birthday 创建成功';
END
GO

-- BoardGames 表索引
-- 分类索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_BoardGames_Category' AND object_id = OBJECT_ID(N'dbo.BoardGames'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BoardGames_Category] ON [dbo].[BoardGames] ([Category] ASC);
    PRINT '索引 IX_BoardGames_Category 创建成功';
END
GO

-- 桌游名称索引（模糊搜索）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_BoardGames_Name' AND object_id = OBJECT_ID(N'dbo.BoardGames'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BoardGames_Name] ON [dbo].[BoardGames] ([Name] ASC);
    PRINT '索引 IX_BoardGames_Name 创建成功';
END
GO

-- 创建时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_BoardGames_CreatedAt' AND object_id = OBJECT_ID(N'dbo.BoardGames'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BoardGames_CreatedAt] ON [dbo].[BoardGames] ([CreatedAt] DESC);
    PRINT '索引 IX_BoardGames_CreatedAt 创建成功';
END
GO

-- 库存查询索引（当前库存 > 0）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_BoardGames_CurrentStock' AND object_id = OBJECT_ID(N'dbo.BoardGames'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BoardGames_CurrentStock] ON [dbo].[BoardGames] ([CurrentStock] DESC);
    PRINT '索引 IX_BoardGames_CurrentStock 创建成功';
END
GO

-- Tables 表索引
-- 区域索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Tables_Area' AND object_id = OBJECT_ID(N'dbo.Tables'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Tables_Area] ON [dbo].[Tables] ([Area] ASC);
    PRINT '索引 IX_Tables_Area 创建成功';
END
GO

-- 状态索引（空闲/占用查询）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Tables_Status' AND object_id = OBJECT_ID(N'dbo.Tables'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Tables_Status] ON [dbo].[Tables] ([Status] ASC);
    PRINT '索引 IX_Tables_Status 创建成功';
END
GO

-- 座位数索引（按人数匹配桌台）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Tables_Seats' AND object_id = OBJECT_ID(N'dbo.Tables'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Tables_Seats] ON [dbo].[Tables] ([Seats] ASC);
    PRINT '索引 IX_Tables_Seats 创建成功';
END
GO

-- Orders 表索引
-- 会员外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Orders_MemberId' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_MemberId] ON [dbo].[Orders] ([MemberId] ASC);
    PRINT '索引 IX_Orders_MemberId 创建成功';
END
GO

-- 桌台外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Orders_TableId' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_TableId] ON [dbo].[Orders] ([TableId] ASC);
    PRINT '索引 IX_Orders_TableId 创建成功';
END
GO

-- 订单状态索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Orders_Status' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_Status] ON [dbo].[Orders] ([Status] ASC);
    PRINT '索引 IX_Orders_Status 创建成功';
END
GO

-- 订单创建时间索引（用于统计报表）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Orders_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_CreatedAt] ON [dbo].[Orders] ([CreatedAt] DESC);
    PRINT '索引 IX_Orders_CreatedAt 创建成功';
END
GO

-- 订单支付时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Orders_PaidAt' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_PaidAt] ON [dbo].[Orders] ([PaidAt] DESC);
    PRINT '索引 IX_Orders_PaidAt 创建成功';
END
GO

-- 复合索引：状态 + 创建时间（常用查询：按时间范围查询待处理订单）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Orders_Status_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_Status_CreatedAt] ON [dbo].[Orders] ([Status] ASC, [CreatedAt] DESC);
    PRINT '索引 IX_Orders_Status_CreatedAt 创建成功';
END
GO

-- OrderItems 表索引
-- 订单外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_OrderItems_OrderId' AND object_id = OBJECT_ID(N'dbo.OrderItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId] ON [dbo].[OrderItems] ([OrderId] ASC);
    PRINT '索引 IX_OrderItems_OrderId 创建成功';
END
GO

-- 桌游外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_OrderItems_BoardGameId' AND object_id = OBJECT_ID(N'dbo.OrderItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OrderItems_BoardGameId] ON [dbo].[OrderItems] ([BoardGameId] ASC);
    PRINT '索引 IX_OrderItems_BoardGameId 创建成功';
END
GO

-- 项目类型索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_OrderItems_Type' AND object_id = OBJECT_ID(N'dbo.OrderItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OrderItems_Type] ON [dbo].[OrderItems] ([Type] ASC);
    PRINT '索引 IX_OrderItems_Type 创建成功';
END
GO

-- Reservations 表索引
-- 会员外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Reservations_MemberId' AND object_id = OBJECT_ID(N'dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Reservations_MemberId] ON [dbo].[Reservations] ([MemberId] ASC);
    PRINT '索引 IX_Reservations_MemberId 创建成功';
END
GO

-- 桌台外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Reservations_TableId' AND object_id = OBJECT_ID(N'dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Reservations_TableId] ON [dbo].[Reservations] ([TableId] ASC);
    PRINT '索引 IX_Reservations_TableId 创建成功';
END
GO

-- 预约状态索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Reservations_Status' AND object_id = OBJECT_ID(N'dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Reservations_Status] ON [dbo].[Reservations] ([Status] ASC);
    PRINT '索引 IX_Reservations_Status 创建成功';
END
GO

-- 预约日期索引（查询某天的预约）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Reservations_Date' AND object_id = OBJECT_ID(N'dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Reservations_Date] ON [dbo].[Reservations] ([Date] ASC);
    PRINT '索引 IX_Reservations_Date 创建成功';
END
GO

-- 复合索引：日期 + 状态（常用查询）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Reservations_Date_Status' AND object_id = OBJECT_ID(N'dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Reservations_Date_Status] ON [dbo].[Reservations] ([Date] ASC, [Status] ASC);
    PRINT '索引 IX_Reservations_Date_Status 创建成功';
END
GO

-- DamageRecords 表索引
-- 桌游外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DamageRecords_BoardGameId' AND object_id = OBJECT_ID(N'dbo.DamageRecords'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DamageRecords_BoardGameId] ON [dbo].[DamageRecords] ([BoardGameId] ASC);
    PRINT '索引 IX_DamageRecords_BoardGameId 创建成功';
END
GO

-- 订单外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DamageRecords_OrderId' AND object_id = OBJECT_ID(N'dbo.DamageRecords'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DamageRecords_OrderId] ON [dbo].[DamageRecords] ([OrderId] ASC);
    PRINT '索引 IX_DamageRecords_OrderId 创建成功';
END
GO

-- 创建时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DamageRecords_CreatedAt' AND object_id = OBJECT_ID(N'dbo.DamageRecords'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DamageRecords_CreatedAt] ON [dbo].[DamageRecords] ([CreatedAt] DESC);
    PRINT '索引 IX_DamageRecords_CreatedAt 创建成功';
END
GO

-- OperationLogs 表索引
-- 用户ID索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_OperationLogs_UserId' AND object_id = OBJECT_ID(N'dbo.OperationLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OperationLogs_UserId] ON [dbo].[OperationLogs] ([UserId] ASC);
    PRINT '索引 IX_OperationLogs_UserId 创建成功';
END
GO

-- 操作时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_OperationLogs_Timestamp' AND object_id = OBJECT_ID(N'dbo.OperationLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OperationLogs_Timestamp] ON [dbo].[OperationLogs] ([Timestamp] DESC);
    PRINT '索引 IX_OperationLogs_Timestamp 创建成功';
END
GO

-- 操作类型索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_OperationLogs_Action' AND object_id = OBJECT_ID(N'dbo.OperationLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_OperationLogs_Action] ON [dbo].[OperationLogs] ([Action] ASC);
    PRINT '索引 IX_OperationLogs_Action 创建成功';
END
GO

-- RechargeRecords 表索引
-- 会员外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_RechargeRecords_MemberId' AND object_id = OBJECT_ID(N'dbo.RechargeRecords'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RechargeRecords_MemberId] ON [dbo].[RechargeRecords] ([MemberId] ASC);
    PRINT '索引 IX_RechargeRecords_MemberId 创建成功';
END
GO

-- 创建时间索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_RechargeRecords_CreatedAt' AND object_id = OBJECT_ID(N'dbo.RechargeRecords'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RechargeRecords_CreatedAt] ON [dbo].[RechargeRecords] ([CreatedAt] DESC);
    PRINT '索引 IX_RechargeRecords_CreatedAt 创建成功';
END
GO

-- 支付方式索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_RechargeRecords_PayMethod' AND object_id = OBJECT_ID(N'dbo.RechargeRecords'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RechargeRecords_PayMethod] ON [dbo].[RechargeRecords] ([PayMethod] ASC);
    PRINT '索引 IX_RechargeRecords_PayMethod 创建成功';
END
GO

-- Coupons 表索引
-- 会员外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Coupons_MemberId' AND object_id = OBJECT_ID(N'dbo.Coupons'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Coupons_MemberId] ON [dbo].[Coupons] ([MemberId] ASC);
    PRINT '索引 IX_Coupons_MemberId 创建成功';
END
GO

-- 使用状态索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Coupons_IsUsed' AND object_id = OBJECT_ID(N'dbo.Coupons'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Coupons_IsUsed] ON [dbo].[Coupons] ([IsUsed] ASC);
    PRINT '索引 IX_Coupons_IsUsed 创建成功';
END
GO

-- 过期日期索引（查询有效优惠券）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Coupons_ExpireDate' AND object_id = OBJECT_ID(N'dbo.Coupons'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Coupons_ExpireDate] ON [dbo].[Coupons] ([ExpireDate] ASC);
    PRINT '索引 IX_Coupons_ExpireDate 创建成功';
END
GO

-- 使用订单外键索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Coupons_UsedOrderId' AND object_id = OBJECT_ID(N'dbo.Coupons'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Coupons_UsedOrderId] ON [dbo].[Coupons] ([UsedOrderId] ASC);
    PRINT '索引 IX_Coupons_UsedOrderId 创建成功';
END
GO

-- 复合索引：会员 + 未使用 + 过期日期（查询会员可用优惠券）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Coupons_MemberId_IsUsed_ExpireDate' AND object_id = OBJECT_ID(N'dbo.Coupons'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Coupons_MemberId_IsUsed_ExpireDate]
    ON [dbo].[Coupons] ([MemberId] ASC, [IsUsed] ASC, [ExpireDate] ASC);
    PRINT '索引 IX_Coupons_MemberId_IsUsed_ExpireDate 创建成功';
END
GO

-- =====================================================
-- 数据库创建完成
-- =====================================================
PRINT '';
PRINT '=========================================';
PRINT '  BoardGameCafe 数据库结构创建完成！';
PRINT '  共创建 11 张数据表及索引、外键约束';
PRINT '  注意：种子数据由 DbInitializer 动态生成';
PRINT '=========================================';
GO
