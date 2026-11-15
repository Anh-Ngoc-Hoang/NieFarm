using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Payments.Commands;

public class ConfirmPaymentFromGatewayCommandValidator : AbstractValidator<ConfirmPaymentFromGatewayCommand>
{
    public ConfirmPaymentFromGatewayCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .MaximumLength(OrderPaymentChange.ProviderMaxLength);

        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .MaximumLength(OrderPaymentChange.ExternalIdMaxLength);

        RuleFor(x => x.Amount).GreaterThan(0);

        // Longer values are not rejected: Reference is truncated by the handler so a long bank
        // reference can never make the gateway retry forever.
    }
}
