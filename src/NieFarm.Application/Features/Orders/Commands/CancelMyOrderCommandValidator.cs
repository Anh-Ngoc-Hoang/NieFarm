using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Commands;

public class CancelMyOrderCommandValidator : AbstractValidator<CancelMyOrderCommand>
{
    public CancelMyOrderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");

        RuleFor(x => x.Reason)
            .MaximumLength(OrderStatusChange.NoteMaxLength)
            .WithMessage("Lý do hủy quá dài (tối đa 500 ký tự).");
    }
}
