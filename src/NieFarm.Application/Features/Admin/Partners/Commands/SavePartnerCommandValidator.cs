using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Partners.Commands;

public class SavePartnerCommandValidator : AbstractValidator<SavePartnerCommand>
{
    public SavePartnerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Partner.NameMaxLength);

        // Defence in depth against a javascript:-style value — logos only ever come from
        // SingleImageField's own upload, which always returns a /images/... path.
        RuleFor(x => x.LogoUrl)
            .MaximumLength(Partner.LogoUrlMaxLength)
            .Must(url => string.IsNullOrEmpty(url) || url.StartsWith("/images/", StringComparison.Ordinal))
            .WithMessage("Logo must be an uploaded image.");

        // The same rule Partner.Update enforces as its last line of defence.
        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(Partner.WebsiteUrlMaxLength)
            .Must(Partner.IsValidWebsiteUrl)
            .WithMessage("Website must be a full address starting with http:// or https://.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
