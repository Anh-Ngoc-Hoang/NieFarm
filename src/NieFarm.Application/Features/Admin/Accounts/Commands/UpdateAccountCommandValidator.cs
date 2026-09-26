using FluentValidation;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30);
    }
}
