using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class RevertOrderPaymentCommandValidator : AbstractValidator<RevertOrderPaymentCommand>
{
    public RevertOrderPaymentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Enter why the payment is being undone.")
            .MaximumLength(OrderPaymentChange.NoteMaxLength)
            .WithMessage($"The reason cannot exceed {OrderPaymentChange.NoteMaxLength} characters.");
    }
}
