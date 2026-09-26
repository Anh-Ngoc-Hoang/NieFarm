using FluentValidation;

namespace NieFarm.Application.Features.Orders.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Vui lòng nhập họ và tên.")
            .MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Vui lòng nhập số điện thoại.")
            .MaximumLength(30)
            .Matches(@"^[0-9+\-\s().]{8,20}$").WithMessage("Số điện thoại không hợp lệ.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Email)
            .MaximumLength(256);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Vui lòng nhập địa chỉ cụ thể.")
            .MaximumLength(200);

        RuleFor(x => x.Ward)
            .NotEmpty().WithMessage("Vui lòng chọn phường/xã.")
            .MaximumLength(120);

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("Vui lòng chọn tỉnh/thành phố.")
            .MaximumLength(120);

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Ghi chú quá dài (tối đa 1000 ký tự).");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Phương thức thanh toán không hợp lệ.");
    }
}
