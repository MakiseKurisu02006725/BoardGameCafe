using FluentValidation;
using BoardGameCafe.Application.DTOs;

namespace BoardGameCafe.Application.Validators;

/// <summary>
/// 登录请求验证器
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    /// <summary>
    /// 初始化登录请求验证规则
    /// </summary>
    public LoginDtoValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号码不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("请输入正确的手机号码格式");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .Length(6, 20).WithMessage("密码长度必须为6-20位");
    }
}

/// <summary>
/// 注册请求验证器
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    /// <summary>
    /// 初始化注册请求验证规则
    /// </summary>
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号码不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("请输入正确的手机号码格式");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .Length(6, 20).WithMessage("密码长度必须为6-20位");

        RuleFor(x => x.NickName)
            .NotEmpty().WithMessage("昵称不能为空")
            .Length(2, 20).WithMessage("昵称长度必须为2-20位");
    }
}
