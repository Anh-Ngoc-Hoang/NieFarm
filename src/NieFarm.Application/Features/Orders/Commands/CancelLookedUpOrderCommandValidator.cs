using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Commands;

public class CancelLookedUpOrderCommandValidator : AbstractValidator<CancelLookedUpOrderCommand>
{
    public CancelLookedUpOrderCommandValidator()
    {
        RuleFor(x => x.Code).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng nhập mã đơn hàng.")
            .MaximumLength(30).WithMessage("Mã đơn hàng không hợp lệ.")
            .Must(c => OrderCode.TryNormalise(c, out _)).WithMessage("Mã đơn hàng không hợp lệ.");

        RuleFor(x => x.Phone).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng nhập số điện thoại.")
            .MaximumLength(30).WithMessage("Số điện thoại không hợp lệ.")
            .Must(p => PhoneNumberNormalizer.Normalise(p).Length is >= 8 and <= 15)
            .WithMessage("Số điện thoại không hợp lệ.");

        RuleFor(x => x.Reason)
            .MaximumLength(OrderStatusChange.NoteMaxLength)
            .WithMessage("Lý do hủy quá dài (tối đa 500 ký tự).");
    }
}
