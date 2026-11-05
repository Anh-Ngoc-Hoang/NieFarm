using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Testimonials.Commands;

public class SaveTestimonialCommandValidator : AbstractValidator<SaveTestimonialCommand>
{
    public SaveTestimonialCommandValidator()
    {
        RuleFor(x => x.Quote)
            .NotEmpty().WithMessage("Quote is required.")
            .MaximumLength(Testimonial.QuoteMaxLength);

        RuleFor(x => x.AuthorName)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(Testimonial.AuthorNameMaxLength);

        RuleFor(x => x.AuthorRole)
            .MaximumLength(Testimonial.AuthorRoleMaxLength);

        // Defence in depth against a javascript:-style value — photos only ever come from
        // SingleImageField's own upload, which always returns a /images/... path.
        RuleFor(x => x.PhotoUrl)
            .MaximumLength(Testimonial.PhotoUrlMaxLength)
            .Must(url => string.IsNullOrEmpty(url) || url.StartsWith("/images/", StringComparison.Ordinal))
            .WithMessage("Photo must be an uploaded image.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
