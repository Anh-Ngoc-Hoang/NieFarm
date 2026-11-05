using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Commands;

public class SaveCoffeeRegionCommandValidator : AbstractValidator<SaveCoffeeRegionCommand>
{
    public SaveCoffeeRegionCommandValidator()
    {
        RuleFor(x => x.Key)
            .IsInEnum().WithMessage("Unknown region.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(CoffeeRegion.TitleMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(CoffeeRegion.DescriptionMaxLength);

        RuleFor(x => x.Tagline).MaximumLength(CoffeeRegion.TaglineMaxLength);
        RuleFor(x => x.Altitude).MaximumLength(CoffeeRegion.AltitudeMaxLength);
        RuleFor(x => x.Varieties).MaximumLength(CoffeeRegion.VarietiesMaxLength);
        RuleFor(x => x.Climate).MaximumLength(CoffeeRegion.ClimateMaxLength);
        RuleFor(x => x.FlavorProfile).MaximumLength(CoffeeRegion.FlavorProfileMaxLength);

        When(x => !string.IsNullOrWhiteSpace(x.ImageUrl), () =>
        {
            RuleFor(x => x.ImageUrl!)
                .MaximumLength(CoffeeRegion.ImageUrlMaxLength)
                .Must(url => url.Trim().StartsWith("/images/", StringComparison.Ordinal))
                .WithMessage("Image must be an uploaded file.");

            RuleFor(x => x.ImageAlt)
                .NotEmpty().WithMessage("Image alt text is required when a photo is set.");
        });

        RuleFor(x => x.ImageAlt).MaximumLength(CoffeeRegion.ImageAltMaxLength);

        RuleFor(x => x.Areas)
            .NotNull().WithMessage("Areas are required.");

        When(x => x.Areas is not null, () =>
        {
            RuleFor(x => x.Areas)
                .Must(areas => areas.Count <= CoffeeRegion.MaxAreas)
                .WithMessage($"A region cannot list more than {CoffeeRegion.MaxAreas} areas.")
                .Must(areas => areas.All(a => a.Id is null or > 0))
                .WithMessage("Area id must be positive.")
                .Must(areas => areas.Where(a => a.Id is not null).Select(a => a.Id).Distinct().Count()
                               == areas.Count(a => a.Id is not null))
                .WithMessage("The same area cannot be listed twice.")
                .Must(areas => areas
                    .Select(a => (a.Name ?? string.Empty).Trim())
                    .Where(n => n.Length > 0)
                    .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .All(g => g.Count() == 1))
                .WithMessage("Area names must be unique within a region.");

            RuleForEach(x => x.Areas).ChildRules(area =>
            {
                area.RuleFor(a => a.Name)
                    .NotEmpty().WithMessage("Area name is required.")
                    .MaximumLength(CoffeeRegionArea.NameMaxLength);

                area.RuleFor(a => a.Note).MaximumLength(CoffeeRegionArea.NoteMaxLength);
            });
        });
    }
}
