using FluentValidation;

namespace NieFarm.Application.Features.Carts.Commands;

public class MergeGuestCartCommandValidator : AbstractValidator<MergeGuestCartCommand>
{
    public MergeGuestCartCommandValidator()
    {
        RuleFor(x => x.AnonymousId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
