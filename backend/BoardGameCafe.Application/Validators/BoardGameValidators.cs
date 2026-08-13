using FluentValidation;
using BoardGameCafe.Application.DTOs;

namespace BoardGameCafe.Application.Validators;

/// <summary>
/// 创建桌游请求验证器
/// </summary>
public class CreateBoardGameDtoValidator : AbstractValidator<CreateBoardGameDto>
{
    /// <summary>
    /// 初始化创建桌游请求验证规则
    /// </summary>
    public CreateBoardGameDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("桌游名称不能为空");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("请选择有效的桌游分类");

        RuleFor(x => x.MinPlayers)
            .GreaterThanOrEqualTo(1).WithMessage("最少玩家数必须大于等于1");

        RuleFor(x => x.TotalStock)
            .GreaterThanOrEqualTo(0).WithMessage("总库存数量必须大于等于0");

        RuleFor(x => x.PricePerHour)
            .GreaterThanOrEqualTo(0).WithMessage("每小时价格必须大于等于0");
    }
}
