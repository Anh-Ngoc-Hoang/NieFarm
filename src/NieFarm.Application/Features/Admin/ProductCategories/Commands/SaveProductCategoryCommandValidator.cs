using FluentValidation;

namespace NieFarm.Application.Features.Admin.ProductCategories.Commands;

public class SaveProductCategoryCommandValidator : AbstractValidator<SaveProductCategoryCommand>
{
    public SaveProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Slug)
            .MaximumLength(160);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
    }
}
