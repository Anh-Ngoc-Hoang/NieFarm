using FluentValidation;

namespace NieFarm.Application.Features.Orders.Commands;

public class ReorderCommandValidator : AbstractValidator<ReorderCommand>
{
    public ReorderCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");
    }
}
