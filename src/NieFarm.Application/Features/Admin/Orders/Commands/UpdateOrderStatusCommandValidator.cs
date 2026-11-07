using FluentValidation;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.FromStatus)
            .IsInEnum().WithMessage("Select a valid order status.");

        RuleFor(x => x.ToStatus)
            .IsInEnum().WithMessage("Select a valid order status.");

        RuleFor(x => x.Note)
            .MaximumLength(OrderStatusChange.NoteMaxLength)
            .WithMessage($"The note cannot exceed {OrderStatusChange.NoteMaxLength} characters.");

        RuleFor(x => x.Note)
            .Must(n => !string.IsNullOrWhiteSpace(n))
            .When(x => x.ToStatus == OrderStatus.Cancelled)
            .WithMessage("Enter a reason for cancelling this order.");
    }
}
