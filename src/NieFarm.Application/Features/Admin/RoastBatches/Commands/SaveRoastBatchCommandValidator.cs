using FluentValidation;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

public class SaveRoastBatchCommandValidator : AbstractValidator<SaveRoastBatchCommand>
{
    public SaveRoastBatchCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");

        RuleFor(x => x.General).NotNull().SetValidator(new GeneralValidator());
        RuleFor(x => x.Origin).NotNull().SetValidator(new OriginValidator());
        RuleFor(x => x.Process).NotNull().SetValidator(new ProcessValidator());
        RuleFor(x => x.Quality).NotNull().SetValidator(new QualityValidator());
        RuleFor(x => x.Brewing).NotNull().SetValidator(new BrewingValidator());

        // Cross-block rules: dates that have to line up with the roast date. The domain enforces the same
        // as its last line of defence.
        RuleFor(x => x).Custom((cmd, context) =>
        {
            if (cmd.General is null || cmd.Origin is null || cmd.Process is null)
                return;

            var roast = cmd.General.RoastDate;
            if (roast == default)
                return;

            if (cmd.Origin.HarvestedFrom is { } hf && hf > roast
                || cmd.Origin.HarvestedTo is { } ht && ht > roast)
            {
                context.AddFailure("Origin.HarvestedTo", "Harvest cannot end after the roast date.");
            }

            if (cmd.Process.PackedOn is { } packed && packed < roast)
                context.AddFailure("Process.PackedOn", "Packing date cannot be earlier than the roast date.");
        });
    }

    // The same rule as the domain guard (RoastBatch.IsValidImageUrl): prefix plus no "..", backslash, query or fragment.
    private static bool IsBatchImage(string? url) => RoastBatch.IsValidImageUrl(url);

    private sealed class GeneralValidator : AbstractValidator<RoastBatchGeneralInput>
    {
        public GeneralValidator()
        {
            RuleFor(x => x.RoastDate).NotEqual(default(DateOnly)).WithMessage("Roast date is required.");

            RuleFor(x => x.BestBefore)
                .Must((x, bb) => bb is null || bb >= x.RoastDate)
                .WithMessage("Best-before cannot be earlier than the roast date.");

            RuleFor(x => x.PeakFromDays).InclusiveBetween(0, RoastBatch.MaxPeakDays);
            RuleFor(x => x.PeakToDays).InclusiveBetween(0, RoastBatch.MaxPeakDays);
            RuleFor(x => x)
                .Must(x => x.PeakFromDays is null || x.PeakToDays is null || x.PeakFromDays <= x.PeakToDays)
                .WithName("Peak window")
                .WithMessage("Peak window start cannot be after its end.");

            RuleFor(x => x.NetWeightGrams).InclusiveBetween(1, RoastBatch.MaxNetWeightGrams);
            RuleFor(x => x.PackCount).InclusiveBetween(1, RoastBatch.MaxPackCount);
            RuleFor(x => x.InternalNote).MaximumLength(RoastBatch.InternalNoteMaxLength);
        }
    }

    private sealed class OriginValidator : AbstractValidator<RoastBatchOriginInput>
    {
        public OriginValidator()
        {
            RuleFor(x => x.Province).MaximumLength(RoastBatch.OriginProvinceMaxLength);
            RuleFor(x => x.Commune).MaximumLength(RoastBatch.OriginCommuneMaxLength);
            RuleFor(x => x.Locality).MaximumLength(RoastBatch.OriginLocalityMaxLength);

            RuleFor(x => x.AltitudeMinMeters).InclusiveBetween(0, RoastBatch.MaxAltitudeMeters);
            RuleFor(x => x.AltitudeMaxMeters).InclusiveBetween(0, RoastBatch.MaxAltitudeMeters);
            RuleFor(x => x)
                .Must(x => x.AltitudeMinMeters is null || x.AltitudeMaxMeters is null
                           || x.AltitudeMinMeters <= x.AltitudeMaxMeters)
                .WithName("Altitude")
                .WithMessage("Minimum altitude cannot exceed the maximum.");

            RuleFor(x => x.Variety).MaximumLength(RoastBatch.VarietyMaxLength);
            RuleFor(x => x.FarmName).MaximumLength(RoastBatch.FarmNameMaxLength);
            RuleFor(x => x.FarmStory).MaximumLength(RoastBatch.FarmStoryMaxLength);

            RuleFor(x => x.FarmImageUrl)
                .MaximumLength(RoastBatch.ImageUrlMaxLength)
                .Must(IsBatchImage).WithMessage("Farm photo must be an uploaded image.");

            RuleFor(x => x.HarvestSeason).MaximumLength(RoastBatch.HarvestSeasonMaxLength);
            RuleFor(x => x.HarvestMethod).MaximumLength(RoastBatch.HarvestMethodMaxLength);

            RuleFor(x => x)
                .Must(x => x.HarvestedFrom is null || x.HarvestedTo is null || x.HarvestedFrom <= x.HarvestedTo)
                .WithName("Harvest window")
                .WithMessage("Harvest start cannot be after its end.");
        }
    }

    private sealed class ProcessValidator : AbstractValidator<RoastBatchProcessInput>
    {
        public ProcessValidator()
        {
            RuleFor(x => x.ProcessingMethod).IsInEnum();
            RuleFor(x => x.ProcessingNote).MaximumLength(RoastBatch.ProcessingNoteMaxLength);
            RuleFor(x => x.FermentationHours).InclusiveBetween(0, RoastBatch.MaxFermentationHours);
            RuleFor(x => x.DryingDays).InclusiveBetween(0, RoastBatch.MaxDryingDays);
            RuleFor(x => x.DryingMethod).MaximumLength(RoastBatch.DryingMethodMaxLength);

            RuleFor(x => x.RoastLevel).IsInEnum();
            RuleFor(x => x.RoasterName).MaximumLength(RoastBatch.RoasterNameMaxLength);
            RuleFor(x => x.RoastNote).MaximumLength(RoastBatch.RoastNoteMaxLength);
            RuleFor(x => x.PackagingNote).MaximumLength(RoastBatch.PackagingNoteMaxLength);
        }
    }

    private sealed class QualityValidator : AbstractValidator<RoastBatchQualityInput>
    {
        public QualityValidator()
        {
            RuleFor(x => x.CuppingScore)
                .Must(s => s is null || (s > 0m && s <= 100m))
                .WithMessage("Cupping score must be above 0 and at most 100.");
            RuleFor(x => x.CuppedBy).MaximumLength(RoastBatch.CuppedByMaxLength);
            // Measured on the stored (rejoined) form, exactly like the domain: "a,b" is stored as "a, b".
            RuleFor(x => x.FlavourNotes)
                .Must(v => RoastBatch.JoinFlavourNotes(v).Length <= RoastBatch.FlavourNotesMaxLength)
                .WithMessage($"Flavour notes are too long (at most {RoastBatch.FlavourNotesMaxLength} characters once tidied).");

            RuleFor(x => x.AcidityLevel).InclusiveBetween((byte)1, (byte)5);
            RuleFor(x => x.BodyLevel).InclusiveBetween((byte)1, (byte)5);
            RuleFor(x => x.SweetnessLevel).InclusiveBetween((byte)1, (byte)5);
            RuleFor(x => x.BitternessLevel).InclusiveBetween((byte)1, (byte)5);

            RuleFor(x => x.MoisturePercent)
                .Must(v => v is null || (v >= 0m && v <= 20m))
                .WithMessage("Moisture must be between 0 and 20 percent.");
            RuleFor(x => x.ScreenSize).MaximumLength(RoastBatch.ScreenSizeMaxLength);
            RuleFor(x => x.DefectRatePercent)
                .Must(v => v is null || (v >= 0m && v <= 100m))
                .WithMessage("Defect rate must be between 0 and 100 percent.");

            RuleFor(x => x.LabReportImageUrl)
                .MaximumLength(RoastBatch.ImageUrlMaxLength)
                .Must(IsBatchImage).WithMessage("Lab report must be an uploaded image.");

            RuleFor(x => x.Certifications)
                .NotNull()
                .Must(c => c is null || c.Count <= RoastBatch.MaxCertifications)
                .WithMessage($"At most {RoastBatch.MaxCertifications} certifications.");

            RuleForEach(x => x.Certifications).ChildRules(cert =>
            {
                cert.RuleFor(c => c.Name)
                    .NotEmpty().WithMessage("Certification name is required.")
                    .MaximumLength(RoastBatchCertification.NameMaxLength);
                cert.RuleFor(c => c.Number).MaximumLength(RoastBatchCertification.NumberMaxLength);
            });
        }
    }

    private sealed class BrewingValidator : AbstractValidator<RoastBatchBrewingInput>
    {
        public BrewingValidator()
        {
            RuleFor(x => x.StorageTip).MaximumLength(RoastBatch.StorageTipMaxLength);

            RuleFor(x => x.Guides)
                .NotNull()
                .Must(g => g is null || g.Count <= RoastBatch.MaxBrewGuides)
                .WithMessage($"At most {RoastBatch.MaxBrewGuides} brew guides.")
                .Must(g => g is null || g.Select(x => x.Method).Distinct().Count() == g.Count)
                .WithMessage("Each brew method can only be listed once.");

            RuleForEach(x => x.Guides).ChildRules(guide =>
            {
                guide.RuleFor(g => g.Method).IsInEnum();
                guide.RuleFor(g => g.Ratio).MaximumLength(RoastBatchBrewGuide.RatioMaxLength);
                guide.RuleFor(g => g.WaterTempCelsius).InclusiveBetween(0, RoastBatchBrewGuide.MaxWaterTempCelsius);
                guide.RuleFor(g => g.BrewTime).MaximumLength(RoastBatchBrewGuide.BrewTimeMaxLength);
                guide.RuleFor(g => g.Note).MaximumLength(RoastBatchBrewGuide.NoteMaxLength);
            });
        }
    }
}
