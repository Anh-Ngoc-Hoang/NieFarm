using FluentValidation;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Select a valid order status.");
    }
}
