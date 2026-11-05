using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

public class SaveCropCommandValidator : AbstractValidator<SaveCropCommand>
{
    public SaveCropCommandValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(Crop.MinYear, Crop.MaxYear);

        RuleFor(x => x.SeasonLabel)
            .NotEmpty().WithMessage("Season window is required.")
            .MaximumLength(Crop.SeasonLabelMaxLength);

        RuleFor(x => x.Headline)
            .NotEmpty().WithMessage("Headline is required.")
            .MaximumLength(Crop.HeadlineMaxLength);

        RuleFor(x => x.Story)
            .NotEmpty().WithMessage("Story is required.")
            .MaximumLength(Crop.StoryMaxLength);

        RuleFor(x => x.Region).MaximumLength(Crop.RegionMaxLength);
        RuleFor(x => x.Altitude).MaximumLength(Crop.AltitudeMaxLength);
        RuleFor(x => x.Varieties).MaximumLength(Crop.VarietiesMaxLength);
        RuleFor(x => x.Process).MaximumLength(Crop.ProcessMaxLength);
        RuleFor(x => x.TastingNotes).MaximumLength(Crop.TastingNotesMaxLength);

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image is required.")
            .MaximumLength(Crop.ImageUrlMaxLength)
            .Must(url => url.StartsWith("/images/", StringComparison.Ordinal))
            .WithMessage("Image must be an uploaded file.");

        RuleFor(x => x.ImageAlt)
            .NotEmpty().WithMessage("Image alt text is required.")
            .MaximumLength(Crop.ImageAltMaxLength);

        RuleFor(x => x.ProductIds)
            .NotNull()
            .Must(ids => ids.Count <= Crop.MaxProducts)
            .WithMessage($"A crop cannot list more than {Crop.MaxProducts} products.")
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Product id must be positive.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("The same product cannot be listed twice.");
    }
}
