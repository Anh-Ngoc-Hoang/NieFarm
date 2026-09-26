using FluentValidation;

namespace NieFarm.Application.Features.Account.Commands;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Vui lòng nhập email.")
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(256);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Vui lòng nhập họ và tên.")
            .MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất một chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất một chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất một chữ số.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Mật khẩu xác nhận không khớp.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30);
    }
}
