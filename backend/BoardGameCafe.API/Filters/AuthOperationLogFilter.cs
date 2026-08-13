using System.Reflection;
using BoardGameCafe.API.Services;
using BoardGameCafe.Application.Services;
using BoardGameCafe.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoardGameCafe.API.Filters;

/// <summary>
/// 操作日志过滤器，记录用户的操作行为（中文描述 + 业务编号目标）
/// </summary>
public class AuthOperationLogFilter : IActionFilter
{
    /// <summary>
    /// 当前用户服务
    /// </summary>
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 操作日志服务
    /// </summary>
    private readonly IOperationLogService _operationLogService;

    /// <summary>
    /// 工作单元（用于查询业务编号）
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 初始化操作日志过滤器的新实例
    /// </summary>
    /// <param name="currentUserService">当前用户服务</param>
    /// <param name="operationLogService">操作日志服务</param>
    /// <param name="unitOfWork">工作单元</param>
    public AuthOperationLogFilter(
        ICurrentUserService currentUserService,
        IOperationLogService operationLogService,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _operationLogService = operationLogService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Action执行前的处理，用于捕获Action参数
    /// </summary>
    /// <param name="context">Action执行上下文</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var parameters = context.ActionDescriptor.Parameters
            .ToDictionary(
                p => p.Name!,
                p => context.ActionArguments.TryGetValue(p.Name!, out var value) ? value : null
            );

        context.HttpContext.Items["ActionParameters"] = parameters;
    }

    /// <summary>
    /// Action执行后的处理，记录操作日志
    /// </summary>
    /// <param name="context">Action执行后上下文</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var userName = _currentUserService.GetUserName() ?? "Anonymous";

            var controllerName = context.ActionDescriptor.RouteValues["controller"] ?? string.Empty;
            var actionName = context.ActionDescriptor.RouteValues["action"] ?? string.Empty;
            var httpMethod = context.HttpContext.Request.Method;

            // 日志清理接口内部会记录一条 System 的 CLEANUP 日志，此处跳过避免重复记录
            if (controllerName == "Logs" && actionName.Contains("Cleanup", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var parameters = context.HttpContext.Items["ActionParameters"] as Dictionary<string, object?>
                             ?? new Dictionary<string, object?>();

            // 派生操作类型（与前端筛选选项一致：LOGIN/LOGOUT/CREATE/UPDATE/DELETE/PAY/REFUND/CONFIRM/CANCEL/EXPORT/PRINT）
            var action = GetActionType(httpMethod, controllerName, actionName);

            // 目标ID：显示业务编号（订单号/会员号/桌游编号/预约号/桌位编号/用户名），无目标时为空字符串（前端显示 -）
            var targetId = ResolveTargetId(controllerName, actionName, parameters);

            // 描述：人类可读的中文描述
            var description = BuildDescription(controllerName, actionName, parameters, targetId);

            _ = _operationLogService.LogAction(userId, userName, action, targetId, description);
        }
        catch
        {
            // 操作日志失败不影响业务主流程
        }
    }

    /// <summary>
    /// 根据控制器和HTTP方法派生简化的操作类型
    /// </summary>
    private static string GetActionType(string httpMethod, string controller, string action)
    {
        if (controller == "Auth")
        {
            if (action.Contains("Logout", StringComparison.OrdinalIgnoreCase)) return "LOGOUT";
            if (action.Contains("Register", StringComparison.OrdinalIgnoreCase)) return "CREATE";
            return "LOGIN";
        }

        if (controller == "Orders")
        {
            if (action.Contains("Refund", StringComparison.OrdinalIgnoreCase)) return "REFUND";
            if (action.Contains("Pay", StringComparison.OrdinalIgnoreCase)) return "PAY";
            if (action.Contains("Cancel", StringComparison.OrdinalIgnoreCase)) return "CANCEL";
        }

        if (controller == "Reservations")
        {
            if (action.Contains("Confirm", StringComparison.OrdinalIgnoreCase)) return "CONFIRM";
            if (action.Contains("Cancel", StringComparison.OrdinalIgnoreCase)) return "CANCEL";
            if (action.Contains("Convert", StringComparison.OrdinalIgnoreCase)) return "CREATE";
        }

        if (controller == "Tables")
        {
            if (action.Contains("Close", StringComparison.OrdinalIgnoreCase)) return "PAY";
            if (action.Contains("Open", StringComparison.OrdinalIgnoreCase)) return "CREATE";
            if (action.Contains("Change", StringComparison.OrdinalIgnoreCase)) return "UPDATE";
        }

        if (controller == "BoardGames")
        {
            if (action.Contains("Borrow", StringComparison.OrdinalIgnoreCase) ||
                action.Contains("Return", StringComparison.OrdinalIgnoreCase) ||
                action.Contains("Damage", StringComparison.OrdinalIgnoreCase))
            {
                return "UPDATE";
            }
        }

        if (controller == "Users")
        {
            if (action.Contains("Password", StringComparison.OrdinalIgnoreCase)) return "UPDATE";
        }

        if (action.Contains("Print", StringComparison.OrdinalIgnoreCase)) return "PRINT";
        if (action.Contains("Export", StringComparison.OrdinalIgnoreCase)) return "EXPORT";

        return httpMethod.ToUpperInvariant() switch
        {
            "POST" => "CREATE",
            "PUT" or "PATCH" => "UPDATE",
            "DELETE" => "DELETE",
            _ => "QUERY"
        };
    }

    /// <summary>
    /// 解析业务编号作为目标ID
    /// </summary>
    /// <param name="controller">控制器名</param>
    /// <param name="action">动作名</param>
    /// <param name="parameters">Action参数</param>
    /// <returns>业务编号，无目标时返回null</returns>
    private string? ResolveTargetId(string controller, string action, Dictionary<string, object?> parameters)
    {
        switch (controller)
        {
            case "Members":
            {
                var memberId = GetDtoGuid(parameters, "MemberId") ?? GetRouteId(parameters);
                if (memberId.HasValue)
                {
                    var member = _unitOfWork.Members.GetById(memberId.Value);
                    if (member != null && !string.IsNullOrEmpty(member.MemberNo)) return member.MemberNo;
                }
                break;
            }
            case "BoardGames":
            {
                var gameId = GetDtoGuid(parameters, "BoardGameId") ?? GetRouteId(parameters);
                if (gameId.HasValue)
                {
                    var game = _unitOfWork.BoardGames.GetById(gameId.Value);
                    if (game != null && !string.IsNullOrEmpty(game.GameNo)) return game.GameNo;
                }
                break;
            }
            case "Reservations":
            {
                var resId = GetRouteId(parameters);
                if (resId.HasValue)
                {
                    var reservation = _unitOfWork.Reservations.GetById(resId.Value);
                    if (reservation != null && !string.IsNullOrEmpty(reservation.ReservationNo)) return reservation.ReservationNo;
                }
                break;
            }
            case "Orders":
            {
                var orderId = GetDtoGuid(parameters, "OrderId") ?? GetRouteId(parameters);
                if (orderId.HasValue)
                {
                    var order = _unitOfWork.Orders.GetById(orderId.Value);
                    if (order != null && !string.IsNullOrEmpty(order.OrderNo)) return order.OrderNo;
                }
                break;
            }
            case "Tables":
            {
                if (action == "OpenTable")
                {
                    var tableId = GetDtoGuid(parameters, "TableId");
                    if (tableId.HasValue)
                    {
                        var order = _unitOfWork.Orders
                            .Find(o => o.TableId == tableId.Value)
                            .OrderByDescending(o => o.CreatedAt)
                            .FirstOrDefault();
                        if (order != null && !string.IsNullOrEmpty(order.OrderNo)) return order.OrderNo;
                    }
                }

                var orderId = GetDtoGuid(parameters, "OrderId") ?? GetDtoGuid(parameters, "CurrentOrderId");
                if (orderId.HasValue)
                {
                    var order = _unitOfWork.Orders.GetById(orderId.Value);
                    if (order != null && !string.IsNullOrEmpty(order.OrderNo)) return order.OrderNo;
                }

                var tableId2 = GetRouteId(parameters) ?? GetDtoGuid(parameters, "TableId") ?? GetDtoGuid(parameters, "NewTableId");
                if (tableId2.HasValue)
                {
                    var table = _unitOfWork.Tables.GetById(tableId2.Value);
                    if (table != null && !string.IsNullOrEmpty(table.Code)) return table.Code;
                }
                break;
            }
            case "Users":
            {
                var userId = GetRouteId(parameters);
                if (userId.HasValue)
                {
                    var user = _unitOfWork.Users.GetById(userId.Value);
                    if (user != null) return user.UserName;
                }
                break;
            }
        }

        return null;
    }

    /// <summary>
    /// 生成人类可读的中文操作描述
    /// </summary>
    /// <param name="controller">控制器名</param>
    /// <param name="action">动作名</param>
    /// <param name="parameters">Action参数</param>
    /// <param name="targetId">业务编号目标</param>
    /// <returns>中文描述</returns>
    private string BuildDescription(string controller, string action, Dictionary<string, object?> parameters, string? targetId)
    {
        switch (controller)
        {
            case "Auth":
                if (action.Contains("Logout", StringComparison.OrdinalIgnoreCase)) return "用户退出系统";
                if (action.Contains("Register", StringComparison.OrdinalIgnoreCase)) return "用户注册";
                if (action.Contains("Refresh", StringComparison.OrdinalIgnoreCase)) return "刷新登录令牌";
                if (action.Contains("GetCurrentUserInfo", StringComparison.OrdinalIgnoreCase)) return "查询当前用户信息";
                return "用户登录系统";

            case "Members":
                switch (action)
                {
                    case "CreateMember": return JoinName("新增会员", GetDtoString(parameters, "NickName"));
                    case "UpdateMember": return "更新会员资料";
                    case "UpdateProfile": return "修改个人资料";
                    case "DeleteMember": return "删除会员";
                    case "Recharge":
                        var rechargeAmount = GetDtoDecimal(parameters, "Amount");
                        return rechargeAmount.HasValue
                            ? $"会员充值 金额 ¥{rechargeAmount.Value:0.##}"
                            : "会员充值";
                    case "GetMembers": return "查询会员列表";
                    case "GetMemberById": return "查询会员详情";
                    case "GetRecharges": return "查询会员充值记录";
                    case "GetOrders": return "查询会员消费记录";
                    case "GetCoupons": return "查询会员优惠券";
                }
                break;

            case "Tables":
                switch (action)
                {
                    case "CreateTable": return JoinName("新增桌位", GetDtoString(parameters, "Code"));
                    case "UpdateTable": return "更新桌位状态";
                    case "DeleteTable": return "删除桌位";
                    case "OpenTable": return targetId != null ? $"创建订单 {targetId}" : "创建订单";
                    case "ChangeTable": return "桌位换台";
                    case "CloseTable": return targetId != null ? $"订单结账 {targetId}" : "订单结账";
                    case "GetTables": return "查询桌位列表";
                    case "GetTableById": return "查询桌位详情";
                }
                break;

            case "Orders":
                switch (action)
                {
                    case "Pay": return targetId != null ? $"支付订单 {targetId}" : "支付订单";
                    case "Refund":
                        var refundOrderId = GetRouteId(parameters);
                        if (refundOrderId.HasValue)
                        {
                            var order = _unitOfWork.Orders.GetById(refundOrderId.Value);
                            if (order != null)
                            {
                                var orderNo = string.IsNullOrEmpty(order.OrderNo) ? "未知订单" : order.OrderNo;
                                return order.ActualAmount > 0
                                    ? $"退款订单 {orderNo} 金额 ¥{order.ActualAmount:0.##}"
                                    : $"退款订单 {orderNo}";
                            }
                        }
                        return "退款订单";
                    case "GetOrders": return "查询订单列表";
                    case "GetOrderById": return "查询订单详情";
                }
                break;

            case "BoardGames":
                switch (action)
                {
                    case "CreateBoardGame": return JoinName("新增桌游", GetDtoString(parameters, "Name"));
                    case "UpdateBoardGame": return "更新桌游信息";
                    case "DeleteBoardGame": return "删除桌游";
                    case "BorrowGame": return JoinName("借出桌游", GetGameName(parameters));
                    case "ReturnGame": return JoinName("归还桌游", GetGameName(parameters));
                    case "RecordDamage": return "登记桌游损坏";
                    case "GetBoardGames": return "查询桌游列表";
                    case "GetBoardGameById": return "查询桌游详情";
                }
                break;

            case "Reservations":
                switch (action)
                {
                    case "CreateReservation": return targetId != null ? $"创建预约 {targetId}" : "创建预约";
                    case "Confirm": return targetId != null ? $"确认预约 {targetId}" : "确认预约";
                    case "Cancel": return targetId != null ? $"取消预约 {targetId}" : "取消预约";
                    case "ConvertToOrder": return targetId != null ? $"预约转开台 {targetId}" : "预约转开台";
                    case "GetReservations": return "查询预约列表";
                    case "GetReservationById": return "查询预约详情";
                }
                break;

            case "Users":
                switch (action)
                {
                    case "CreateUser": return JoinName("新增员工", GetDtoString(parameters, "UserName"));
                    case "UpdateUser": return "更新员工信息";
                    case "DeleteUser": return "删除员工";
                    case "ChangePassword": return "修改登录密码";
                    case "ResetPassword": return "重置员工密码";
                    case "GetUsers": return "查询员工列表";
                    case "GetUserById": return "查询员工详情";
                }
                break;

            case "Statistics":
                switch (action)
                {
                    case "ExportExcel":
                        var reportType = GetDtoString(parameters, "reportType");
                        return reportType switch
                        {
                            "member" => "导出会员列表",
                            "order" => "导出订单列表",
                            _ => reportType != null ? $"导出报表 {reportType}" : "导出报表"
                        };
                    case "GetDashboard": return "查看经营数据";
                    case "GetRevenueTrend": return "查看营业额趋势";
                    case "GetPopularGames": return "查看热门桌游";
                    case "GetMemberRfm": return "查看会员RFM分析";
                }
                break;

            case "Damage":
                if (action == "GetDamages") return "查询损坏记录";
                break;

            case "Logs":
                if (action == "GetLogs") return "查询操作日志";
                break;
        }

        return $"{controller} {action}";
    }

    /// <summary>
    /// 获取路由id参数
    /// </summary>
    private static Guid? GetRouteId(Dictionary<string, object?> parameters)
    {
        if (parameters.TryGetValue("id", out var idValue) &&
            idValue != null &&
            Guid.TryParse(idValue.ToString(), out var id))
        {
            return id;
        }

        return null;
    }

    /// <summary>
    /// 从Action参数或DTO属性中提取Guid值
    /// </summary>
    private static Guid? GetDtoGuid(Dictionary<string, object?> parameters, string propertyName)
    {
        if (parameters.TryGetValue(propertyName, out var direct) &&
            direct != null &&
            Guid.TryParse(direct.ToString(), out var parsed))
        {
            return parsed;
        }

        foreach (var value in parameters.Values)
        {
            if (value == null) continue;

            var prop = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) continue;

            var v = prop.GetValue(value);
            if (v is Guid g && g != Guid.Empty) return g;
        }

        return null;
    }

    /// <summary>
    /// 从Action参数或DTO属性中提取字符串值
    /// </summary>
    private static string? GetDtoString(Dictionary<string, object?> parameters, string propertyName)
    {
        if (parameters.TryGetValue(propertyName, out var direct) && direct != null)
        {
            return direct is Guid ? null : direct.ToString();
        }

        foreach (var value in parameters.Values)
        {
            if (value == null) continue;

            var prop = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) continue;

            return prop.GetValue(value)?.ToString();
        }

        return null;
    }

    /// <summary>
    /// 从DTO属性中提取decimal值
    /// </summary>
    private static decimal? GetDtoDecimal(Dictionary<string, object?> parameters, string propertyName)
    {
        foreach (var value in parameters.Values)
        {
            if (value == null) continue;

            var prop = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) continue;

            var v = prop.GetValue(value);
            if (v is decimal d) return d;
            if (v is int i) return i;
            if (v is long l) return l;
            if (v is double db) return (decimal)db;
            return null;
        }

        return null;
    }

    /// <summary>
    /// 获取桌游名称（借出/归还时通过路由id查询）
    /// </summary>
    private string? GetGameName(Dictionary<string, object?> parameters)
    {
        var gameId = GetRouteId(parameters);
        if (gameId.HasValue)
        {
            var game = _unitOfWork.BoardGames.GetById(gameId.Value);
            return game?.Name;
        }

        return null;
    }

    /// <summary>
    /// 拼接前缀与名称，名称为空时只返回前缀
    /// </summary>
    private static string JoinName(string prefix, string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? prefix : $"{prefix} {name}";
    }
}
