using FluentValidation;

namespace NieFarm.Application.Features.Admin.Articles.Commands;

public class SaveArticleCommandValidator : AbstractValidator<SaveArticleCommand>
{
    public SaveArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(250);

        RuleFor(x => x.Slug)
            .MaximumLength(270);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(150);

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary is required.")
            .MaximumLength(600);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500);

        RuleFor(x => x.HeroImageUrl)
            .MaximumLength(500);

        RuleFor(x => x.ImageAlt)
            .MaximumLength(300);

        RuleFor(x => x.HeroImageAlt)
            .MaximumLength(300);

        RuleFor(x => x.PublishedOn)
            .NotEqual(default(DateOnly)).WithMessage("Publish date is required.");
    }
}
