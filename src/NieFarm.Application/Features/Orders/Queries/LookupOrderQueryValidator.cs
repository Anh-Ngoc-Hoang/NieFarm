using FluentValidation;

namespace NieFarm.Application.Features.Orders.Queries;

public class LookupOrderQueryValidator : AbstractValidator<LookupOrderQuery>
{
    public LookupOrderQueryValidator()
    {
        RuleFor(q => q.Code).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng nhập mã đơn hàng.")
            .MaximumLength(30).WithMessage("Mã đơn hàng không hợp lệ.")
            .Must(c => OrderCode.TryNormalise(c, out _)).WithMessage("Mã đơn hàng không hợp lệ.");

        RuleFor(q => q.Phone).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng nhập số điện thoại.")
            .MaximumLength(30).WithMessage("Số điện thoại không hợp lệ.")
            .Must(p => PhoneNumberNormalizer.Normalise(p).Length is >= 8 and <= 15)
            .WithMessage("Số điện thoại không hợp lệ.");
    }
}
