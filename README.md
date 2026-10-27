# 桌游馆管理系统（BoardGameCafe）

一款面向桌游咖啡馆的 Web 运营管理系统，用于解决门店日常运营中的**会员、预约、桌位、桌游、收银、数据统计**等环节靠手工/纸质管理、信息不透明、账目易出错的问题。

- 前端：Vue 3 + Vite + Element Plus + Pinia + ECharts（端口 5173）
- 后端：ASP.NET Core 8 Web API + EF Core 8 + JWT 双令牌认证（端口 5000）
- 数据库：SQL Server（启动时自动建库建表并填充种子数据）
- 定时任务：Hangfire（生日优惠券）+ BackgroundService（日志自动清理）

## 一、项目解决什么问题

桌游馆日常经营中，会员办卡、桌位开台计时、桌游借还、预约排期、结账优惠、营业统计等环节长期依赖人工记录，容易出现：

- 桌位状态不透明（空闲/占用/预约/维修），到店后无法快速安排
- 桌游借出/归还靠登记簿，库存不准、丢失难追查
- 预约冲突无法提前发现，客户到店无位
- 结账时折扣、优惠券、积分、余额计算繁琐易错
- 营业额、会员消费、桌游热度等经营数据无法自动汇总分析

本系统将上述流程线上化：**开台即计费、结账自动算优惠、桌游借还联动库存、预约自动检测冲突、统计大屏自动汇总**，让门店运营数据实时、准确、可追溯。

## 二、主要功能

| 模块 | 功能说明 |
|------|----------|
| 登录认证 | 手机号+密码登录、JWT 双令牌（Access+Refresh，过期自动刷新）、会员自助注册 |
| 经营大屏 | 营业数据图表总览（Dashboard） |
| 桌位管理 | 桌位增删改、**实时看板**（空闲/占用/预约/维修状态）、开台、换台、结账、确认入座、设置/解除维修 |
| 桌游管理 | 桌游档案与分类、库存（总/当前/损坏）、**借出/归还/补货/损坏登记**、借出中桌游及关联订单查看 |
| 会员管理 | 会员列表/详情、会员等级、积分、**余额充值/消费流水**、会员自助档案 |
| 预约管理 | 线上预约、**时段冲突检测**、确认/取消/完成、预约转开台（自动归还桌游） |
| 订单收银 | 订单列表/详情（含游玩桌游）、**开台结账（单接口算费）**、微信/支付宝/现金/余额支付、退款 |
| 计费规则 | 桌位费=小时数×费率、**折扣折数 0.1~10**、优惠券、积分抵扣、会员余额抵扣 |
| 员工管理 | 员工账号、角色权限（管理员/店长/收银员）、修改密码、管理员重置密码 |
| 数据分析 | 经营统计、RFM 会员分析、热销排行 |
| 操作日志 | 登录/增删改操作审计、日志清理配置 |

### 核心业务规则（与实现一致）

- **开台必须选择至少一款游玩桌游**：开台即扣减桌游库存并登记订单-桌游关联，结账/退款自动归还库存
- **结账单一接口算费**：`closeTable` 一次完成金额计算、优惠券/积分/余额抵扣、桌位释放、桌游归还
- **预约自动检测冲突**：同区域同时段存在已确认预约时拒绝创建；维修桌位不可预约
- **折扣合法性**：开台折扣范围 0.1~10 折，后端校验拒绝非法值
- **会员余额**：余额支付自动按应付金额扣款（不足拒绝），退款自动退回余额并记录流水

## 三、安装方法

### 方式一：Docker Compose（推荐，一键启动）

前置要求：Docker + Docker Compose。

> 全新 clone 后无需任何手工配置：数据库连接串、JWT 密钥、Issuer/Audience 均已由 `docker-compose.yml` 以环境变量注入，首次启动会自动建库建表并灌入种子数据。

```bash
cd BoardGameCafe
docker-compose up -d
docker-compose ps
```

启动后访问：

| 服务 | 地址 |
|------|------|
| 前端 | http://localhost |
| 后端 API | http://localhost:5000/api |
| Swagger 文档 | http://localhost:5000/swagger |

### 方式二：本地开发

前置要求：.NET 8 SDK、Node.js 18+、SQL Server 2022。

**1. 生成配置文件（含数据库连接串）**

出于安全考虑，仓库不提交真实配置 `appsettings.json`（内含数据库密码），需从示例文件复制一份：

```bash
# Windows（在项目根目录执行）
copy backend\BoardGameCafe.API\appsettings.Example.json backend\BoardGameCafe.API\appsettings.json

# macOS / Linux
cp backend/BoardGameCafe.API/appsettings.Example.json backend/BoardGameCafe.API/appsettings.json
```

然后打开 `backend/BoardGameCafe.API/appsettings.json`，把连接串里的 `Password=Your_password123` 改成你本机 SQL Server 的 sa 密码（`DefaultConnection` 和 `HangfireConnection` 共两处），其余不用改。

> 说明：数据库无需手动建库建表。后端启动时 `DbInitializer` 会自动 `EnsureCreated` 建库建表、补齐历史列并填充种子数据（用户/桌位/桌游），且操作幂等，重复启动不会重复插入。

**2. 启动后端**

```bash
cd backend
dotnet run --project BoardGameCafe.API
```

后端启动后：API 地址 http://localhost:5000，Swagger 文档 http://localhost:5000/swagger，Hangfire 面板 http://localhost:5000/hangfire。

**3. 启动前端**

```bash
cd frontend
npm install
npm run dev
```

前端启动后访问 http://localhost:5173（开发环境 API 地址为 `/api`，由 Vite 代理到 5000 端口）。

> 可选：如需自定义接口地址或页面标题，先执行 `copy .env.example .env.development`（macOS/Linux 用 `cp .env.example .env.development`）再修改。不创建也能正常运行。

## 四、使用方法

### 默认账号（种子数据）

| 角色 | 用户名（手机号） | 密码 | 权限范围 |
|------|----------------|------|----------|
| 管理员 | 13800000000 | admin123 | 全部权限 |
| 店长 | 13800000001 | manager123 | 门店运营管理 |
| 收银员 | 13800000002 | cashier123 | 收银、开台、桌游借还 |
| 收银员 | 13800000003 | staff123 | 收银、开台、桌游借还 |

### 典型业务流程

1. **登录**：手机号+密码登录 → 进入经营大屏
2. **桌游管理**：维护桌游档案与库存（新增/编辑/补货）
3. **预约**：预约管理页创建预约（选桌位类型/桌游/时段）→ 确认 → 到店"确认入座"转开台
4. **开台**：实时看板点空闲桌位【开台】→ 第一步选择游玩桌游（库存 0 不可选）→ 第二步填会员/人数/折扣 → 确认开台（桌游库存自动扣减）
5. **结账**：看板点【结账】或订单列表进入收银 → 选择支付方式/优惠券/积分 → 结账完成后桌游库存自动归还
6. **退款**：订单列表对已支付订单点【退款】→ 余额退回、桌游库存归还、桌位释放
7. **数据分析**：查看经营统计、RFM 会员分析、桌游热销排行

## 五、输入输出示例

以下示例均通过 Swagger/HTTP 直接调用后端接口验证。

### 示例 1：登录获取令牌

```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{"phone":"13800000000","password":"admin123"}
```

响应：

```json
{
  "code": 200,
  "message": "登录成功",
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "qEwGjpG2Wmd3O11nH+...",
    "expiresIn": 28800,
    "userId": "7b0e2957-b5da-4781-ae12-9d5c085343b6",
    "role": "Admin",
    "nickName": "admin"
  }
}
```

后续请求需携带请求头 `Authorization: Bearer <accessToken>`。

### 示例 2：桌位列表（实时状态）

```
GET http://localhost:5000/api/tables
Authorization: Bearer <accessToken>
```

响应（节选）：

```json
{
  "code": 200,
  "message": "查询成功",
  "data": [
    {
      "id": "24b82e2c-1dba-42cb-acb0-a31e41f9fcae",
      "code": "A01",
      "area": "Hall",
      "areaName": "大厅",
      "seats": 4,
      "status": "Idle",
      "statusName": "空闲",
      "hourlyRate": 3.00
    }
  ]
}
```

### 示例 3：开台（选择游玩桌游，自动扣减库存）

```
POST http://localhost:5000/api/tables/open
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "tableId": "24b82e2c-1dba-42cb-acb0-a31e41f9fcae",
  "guestCount": 2,
  "discount": 9,
  "boardGameIds": ["0c280a8d-275a-418b-9277-175b4ff09a89"]
}
```

响应：返回新建订单（`discountRate: 9`，桌游库存 -1，登记订单-桌游关联）。

```json
{
  "code": 200,
  "message": "开台成功",
  "data": {
    "id": "ad4b5bb7-1c0c-47e6-b290-dd88b6a6c622",
    "orderNo": "ORD20260812012",
    "tableCode": "B02",
    "status": "Pending",
    "statusName": "待支付",
    "discountRate": 9.00
  }
}
```

> 非法折扣会被拒绝：`discount: 11` 时返回 400「折扣范围仅支持0.1~10.0折」。

### 示例 4：结账关台（金额计算 + 归还桌游）

```
POST http://localhost:5000/api/tables/close
Authorization: Bearer <accessToken>
Content-Type: application/json

{"orderId": "ad4b5bb7-1c0c-47e6-b290-dd88b6a6c622", "paymentMethod": "Cash"}
```

响应：桌位费 25 元、9 折 → 折扣 2.50、实收 22.50，桌游库存自动 +1。

```json
{
  "code": 200,
  "message": "结账成功",
  "data": {
    "orderNo": "ORD20260812012",
    "totalAmount": 25.00,
    "discountRate": 9.00,
    "discount": 2.50,
    "actualAmount": 22.50,
    "status": "Paid",
    "statusName": "已支付",
    "paymentMethod": "Cash"
  }
}
```

### 示例 5：桌游列表（分页）

```
GET http://localhost:5000/api/boardgames?pageIndex=1&pageSize=10&keyword=狼
Authorization: Bearer <accessToken>
```

响应（分页字段为 `totalCount` / `items`）：

```json
{
  "code": 200,
  "message": "查询成功",
  "data": {
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1,
    "items": [
      {
        "id": "53cc7824-7f17-4fcc-97d7-39a2d06b2934",
        "name": "狼人杀",
        "gameNo": "B0002",
        "currentStock": 5,
        "totalStock": 5,
        "pricePerHour": 10.00
      }
    ]
  }
}
```

## 目录结构

```
BoardGameCafe/
├── backend/                              # 后端解决方案
│   ├── BoardGameCafe.sln
│   ├── BoardGameCafe.API/                # API 层（Controllers、Program、中间件、DbInitializer、定时任务）
│   ├── BoardGameCafe.Application/        # 应用层（业务服务、DTO、映射、校验）
│   ├── BoardGameCafe.Core/               # 领域层（实体、枚举、仓储接口）
│   ├── BoardGameCafe.Infrastructure/     # 基础设施层（EF DbContext、仓储实现）
│   ├── BoardGameCafe.Common/             # 公共层（响应封装、异常、工具）
│   └── database.sql                      # 数据库参考脚本（可选，启动时会自动建库）
├── frontend/                             # 前端项目（Vue 3 + Vite + Element Plus）
│   ├── src/
│   │   ├── api/                          # API 接口封装
│   │   ├── components/                   # 公共组件（DataTable、SearchBar、StatusTag 等）
│   │   ├── router/                       # 动态路由配置
│   │   ├── store/                        # Pinia 状态管理
│   │   └── views/                        # 业务页面
│   │       ├── boardgames/               # 桌游管理
│   │       ├── dashboard/                # 经营大屏
│   │       ├── members/                  # 会员管理
│   │       ├── orders/                   # 订单收银
│   │       ├── reservations/             # 预约管理
│   │       ├── statistics/               # 数据分析
│   │       ├── tables/                   # 桌位管理（列表 + 实时看板）
│   │       ├── users/                    # 员工管理
│   │       ├── logs/                     # 操作日志
│   │       └── Login/Register/NotFound 等
│   └── Dockerfile
├── docker-compose.yml                    # 一键编排（SQL Server + 后端 + 前端）
└── README.md
```

## 常见问题

- **端口占用**：后端默认 5000，前端默认 5173，可在 `launchSettings.json` / `package.json` 中调整。
- **数据库自动初始化失败**：检查 `appsettings.Development.json` 连接字符串是否指向可用的 SQL Server 实例。
- **接口地址 404**：前端开发环境通过 Vite 代理将 `/api` 转发至后端 5000 端口；如单独部署需修改 `frontend/.env.production` 中的 `VITE_API_BASE_URL`。

## 许可证

本项目基于 **BSD 3-Clause License** 开源，详见 [LICENSE](LICENSE)。
