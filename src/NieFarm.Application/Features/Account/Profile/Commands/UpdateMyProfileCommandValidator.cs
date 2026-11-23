using FluentValidation;

namespace NieFarm.Application.Features.Account.Profile.Commands;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(AccountMessages.NameRequired)
            .MaximumLength(150).WithMessage(AccountMessages.NameTooLong);

        // Same pattern as the checkout form, so a saved phone is always usable there.
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage(AccountMessages.PhoneInvalid)
            .Matches(@"^[0-9+\-\s().]{8,20}$").WithMessage(AccountMessages.PhoneInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
