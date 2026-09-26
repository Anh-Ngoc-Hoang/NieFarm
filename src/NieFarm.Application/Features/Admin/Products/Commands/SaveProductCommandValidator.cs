using FluentValidation;
using NieFarm.Application.Features.Admin.Products.Dtos;

namespace NieFarm.Application.Features.Admin.Products.Commands;

public class SaveProductCommandValidator : AbstractValidator<SaveProductCommand>
{
    public SaveProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .MaximumLength(220);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.")
            .LessThan(1_000_000_000).WithMessage("Price is unrealistically large.");

        RuleFor(x => x.OldPrice)
            .GreaterThan(x => x.Price)
            .When(x => x.OldPrice.HasValue)
            .WithMessage("Old price must be greater than the current price.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500);

        RuleFor(x => x.ImageAlt)
            .MaximumLength(300);

        // Rich text: the limit has to leave room for the markup around the words.
        RuleFor(x => x.Summary)
            .MaximumLength(2000).WithMessage("Summary is too long — keep it under 2000 characters including formatting.");

        RuleFor(x => x.Badge)
            .MaximumLength(50);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");

        // Specs and features are flat lists with no cross-list invariants, so ordinary rules
        // cover them — no shared helper is warranted the way it is for the variant matrix.
        RuleFor(x => x.Specs)
            .Must(specs => specs is null || specs.Count <= ProductSpecLimits.MaxSpecs)
            .WithMessage($"A product can have at most {ProductSpecLimits.MaxSpecs} specifications.");

        RuleForEach(x => x.Specs).ChildRules(spec =>
        {
            spec.RuleFor(s => s.Label)
                .MaximumLength(ProductSpecLimits.MaxLabelLength)
                .WithMessage($"A specification label is longer than {ProductSpecLimits.MaxLabelLength} characters.");

            spec.RuleFor(s => s.Value)
                .MaximumLength(ProductSpecLimits.MaxValueLength)
                .WithMessage($"A specification value is longer than {ProductSpecLimits.MaxValueLength} characters.");
        });

        RuleFor(x => x.Features)
            .Must(features => features is null || features.Count <= ProductSpecLimits.MaxFeatures)
            .WithMessage($"A product can have at most {ProductSpecLimits.MaxFeatures} key features.");

        RuleForEach(x => x.Features)
            .MaximumLength(ProductSpecLimits.MaxFeatureLength)
            .WithMessage($"A key feature is longer than {ProductSpecLimits.MaxFeatureLength} characters.");

        RuleFor(x => x.GalleryImageUrls)
            .Must(list => (list?.Count ?? 0) <= ProductImageLimits.MaxImages)
            .WithMessage($"A product can have at most {ProductImageLimits.MaxImages} gallery images.");

        // The matrix is checked as a whole: the rules are about how options and variants line
        // up with each other, not about either list on its own.
        RuleFor(x => x).Custom((command, context) =>
        {
            foreach (var message in ProductVariantValidation.Validate(command.Options, command.Variants))
                context.AddFailure(message);
        });
    }
}
