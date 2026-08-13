using AutoMapper;
using BoardGameCafe.Application.DTOs;
using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Enums;

namespace BoardGameCafe.Application.MappingProfiles;

/// <summary>
/// 通用映射配置文件
/// </summary>
public class GeneralProfile : Profile
{
    /// <summary>
    /// 初始化映射配置
    /// </summary>
    public GeneralProfile()
    {
        CreateUserMaps();
        CreateMemberMaps();
        CreateBoardGameMaps();
        CreateTableMaps();
        CreateOrderMaps();
        CreateReservationMaps();
        CreateDamageRecordMaps();
        CreateOperationLogMaps();
        CreateRechargeRecordMaps();
    }

    /// <summary>
    /// 配置用户相关映射
    /// </summary>
    private void CreateUserMaps()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => GetRoleName(src.Role)));

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }

    /// <summary>
    /// 配置会员相关映射
    /// </summary>
    private void CreateMemberMaps()
    {
        CreateMap<Member, MemberDto>()
            .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => GetMemberLevelName(src.Level)));

        CreateMap<RechargeRecord, RechargeRecordDto>();
    }

    /// <summary>
    /// 配置桌游相关映射
    /// </summary>
    private void CreateBoardGameMaps()
    {
        CreateMap<BoardGame, BoardGameDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => GetGameCategoryName(src.Category)));

        CreateMap<CreateBoardGameDto, BoardGame>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentStock, opt => opt.MapFrom(src => src.TotalStock))
            .ForMember(dest => dest.DamagedStock, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MaxPlayers, opt => opt.MapFrom(src => src.MaxPlayers ?? src.MinPlayers));
    }

    /// <summary>
    /// 配置桌位相关映射
    /// </summary>
    private void CreateTableMaps()
    {
        CreateMap<Table, TableDto>()
            .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => GetTableAreaName(src.Area)))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetTableStatusName(src.Status)));

        CreateMap<CreateTableDto, Table>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TableStatus.Idle))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }

    /// <summary>
    /// 配置订单相关映射
    /// </summary>
    private void CreateOrderMaps()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetOrderStatusName(src.Status)))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src =>
                src.EndTime.HasValue ? (int)(src.EndTime.Value - src.StartTime).TotalMinutes : (int?)null));

        CreateMap<Order, OrderDetailDto>()
            .IncludeBase<Order, OrderDto>();

        CreateMap<OrderItem, OrderItemDto>();
    }

    /// <summary>
    /// 配置预约相关映射
    /// </summary>
    private void CreateReservationMaps()
    {
        CreateMap<Reservation, ReservationDto>();

        CreateMap<CreateReservationDto, Reservation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MemberId, opt => opt.Ignore())
            .ForMember(dest => dest.TableId, opt => opt.Ignore())
            .ForMember(dest => dest.Deposit, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ReservationStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }

    /// <summary>
    /// 配置损坏记录相关映射
    /// </summary>
    private void CreateDamageRecordMaps()
    {
        CreateMap<DamageRecord, DamageRecordDto>();

        CreateMap<CreateDamageDto, DamageRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }

    /// <summary>
    /// 配置操作日志相关映射
    /// </summary>
    private void CreateOperationLogMaps()
    {
        CreateMap<OperationLog, OperationLogDto>();
    }

    /// <summary>
    /// 配置充值记录相关映射
    /// </summary>
    private void CreateRechargeRecordMaps()
    {
        CreateMap<RechargeRecord, RechargeRecordDto>();
    }

    /// <summary>
    /// 获取用户角色名称
    /// </summary>
    private static string GetRoleName(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "管理员",
            UserRole.Manager => "店长/经理",
            UserRole.Cashier => "收银员",
            UserRole.Member => "普通会员",
            _ => "未知"
        };
    }

    /// <summary>
    /// 获取会员等级名称
    /// </summary>
    private static string GetMemberLevelName(MemberLevel level)
    {
        return level switch
        {
            MemberLevel.Bronze => "青铜会员",
            MemberLevel.Silver => "白银会员",
            MemberLevel.Gold => "黄金会员",
            _ => "未知等级"
        };
    }

    /// <summary>
    /// 获取桌游分类名称
    /// </summary>
    private static string GetGameCategoryName(GameCategory category)
    {
        return category switch
        {
            GameCategory.Strategy => "策略类",
            GameCategory.Party => "派对类",
            GameCategory.Family => "家庭类",
            GameCategory.Children => "儿童类",
            GameCategory.Cooperation => "合作类",
            GameCategory.WarGame => "战棋类",
            GameCategory.CardGame => "卡牌类",
            _ => "未分类"
        };
    }

    /// <summary>
    /// 获取桌位区域名称
    /// </summary>
    private static string GetTableAreaName(TableArea area)
    {
        return area switch
        {
            TableArea.Hall => "大厅",
            TableArea.PrivateRoom => "包间",
            TableArea.Booth => "卡座",
            _ => "未知区域"
        };
    }

    /// <summary>
    /// 获取桌位状态名称
    /// </summary>
    private static string GetTableStatusName(TableStatus status)
    {
        return status switch
        {
            TableStatus.Idle => "空闲",
            TableStatus.Occupied => "占用中",
            TableStatus.Reserved => "已预约",
            TableStatus.Maintenance => "维护中",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 获取订单状态名称
    /// </summary>
    private static string GetOrderStatusName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "待支付",
            OrderStatus.Paid => "已支付",
            OrderStatus.Cancelled => "已取消",
            OrderStatus.Refunded => "已退款",
            _ => "未知状态"
        };
    }
}
