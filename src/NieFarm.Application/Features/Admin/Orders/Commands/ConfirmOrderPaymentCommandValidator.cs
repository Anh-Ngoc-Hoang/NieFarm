using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class ConfirmOrderPaymentCommandValidator : AbstractValidator<ConfirmOrderPaymentCommand>
{
    public ConfirmOrderPaymentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.ExpectedStatus)
            .IsInEnum().WithMessage("Select a valid order status.");

        RuleFor(x => x.Reference)
            .MaximumLength(OrderPaymentChange.ReferenceMaxLength)
            .WithMessage($"The reference cannot exceed {OrderPaymentChange.ReferenceMaxLength} characters.");

        RuleFor(x => x.Note)
            .MaximumLength(OrderPaymentChange.NoteMaxLength)
            .WithMessage($"The note cannot exceed {OrderPaymentChange.NoteMaxLength} characters.");
    }
}
