using NieFarm.Application.Features.Crops;
using NieFarm.Application.Features.Traceability.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability;

/// <summary>
/// Turns a <see cref="RoastBatch"/> (with <c>Product</c>, certifications and brew guides loaded) into the public
/// DTO. This is the only place that decides which blocks are hidden (R-26 D8): an empty string becomes null and a
/// block whose fields are all empty becomes null. <c>InternalNote</c> and <c>PackCount</c> never leave here.
/// </summary>
public static class BatchTraceMapper
{
    public static BatchTraceDto ToDto(RoastBatch batch)
    {
        var product = batch.Product
            ?? throw new InvalidOperationException("The batch must be loaded with its product.");

        return new BatchTraceDto(
            batch.Id,
            batch.IsPublished,
            batch.Code,
            BatchCode.Format(batch.Code),
            new TraceProductDto(
                product.Name,
                product.IsVisible ? Opt(product.Slug) : null,
                Opt(product.ImageUrl),
                Opt(product.ImageAlt) ?? product.Name),
            new TraceVerificationDto(
                batch.RoastDate,
                batch.BestBefore,
                batch.PeakFromDays is { } from ? batch.RoastDate.AddDays(from) : null,
                batch.PeakToDays is { } to ? batch.RoastDate.AddDays(to) : null,
                batch.NetWeightGrams),
            MapOrigin(batch),
            MapHarvest(batch),
            MapProcessing(batch),
            new TraceRoastStepDto(batch.RoastDate, batch.RoastLevel, Opt(batch.RoasterName), Opt(batch.RoastNote)),
            MapPacking(batch),
            MapQuality(batch),
            MapBrewing(batch));
    }

    private static TraceOriginDto? MapOrigin(RoastBatch b)
    {
        var paragraphs = CropText.SplitParagraphs(b.FarmStory);

        var hasAny = Has(b.OriginProvince) || Has(b.OriginCommune) || Has(b.OriginLocality)
            || b.AltitudeMinMeters is not null || b.AltitudeMaxMeters is not null
            || Has(b.Variety) || Has(b.FarmName) || paragraphs.Count > 0 || Has(b.FarmImageUrl);

        return hasAny
            ? new TraceOriginDto(
                Opt(b.OriginProvince), Opt(b.OriginCommune), Opt(b.OriginLocality),
                b.AltitudeMinMeters, b.AltitudeMaxMeters,
                Opt(b.Variety), Opt(b.FarmName), paragraphs, Opt(b.FarmImageUrl))
            : null;
    }

    private static TraceHarvestStepDto? MapHarvest(RoastBatch b) =>
        Has(b.HarvestSeason) || b.HarvestedFrom is not null || b.HarvestedTo is not null || Has(b.HarvestMethod)
            ? new TraceHarvestStepDto(Opt(b.HarvestSeason), b.HarvestedFrom, b.HarvestedTo, Opt(b.HarvestMethod))
            : null;

    private static TraceProcessingStepDto? MapProcessing(RoastBatch b) =>
        b.ProcessingMethod is not null || Has(b.ProcessingNote) || b.FermentationHours is not null
            || b.DryingDays is not null || Has(b.DryingMethod)
            ? new TraceProcessingStepDto(
                b.ProcessingMethod, Opt(b.ProcessingNote), b.FermentationHours, b.DryingDays, Opt(b.DryingMethod))
            : null;

    private static TracePackingStepDto? MapPacking(RoastBatch b) =>
        b.PackedOn is not null || b.HasOneWayValve || b.IsNitrogenFlushed || Has(b.PackagingNote)
            ? new TracePackingStepDto(b.PackedOn, b.HasOneWayValve, b.IsNitrogenFlushed, Opt(b.PackagingNote))
            : null;

    private static TraceQualityDto? MapQuality(RoastBatch b)
    {
        var flavours = CropText.SplitTastingNotes(b.FlavourNotes);

        var sensory = b.AcidityLevel is not null || b.BodyLevel is not null
            || b.SweetnessLevel is not null || b.BitternessLevel is not null
            ? new TraceSensoryDto(b.AcidityLevel, b.BodyLevel, b.SweetnessLevel, b.BitternessLevel)
            : null;

        var specs = b.MoisturePercent is not null || Has(b.ScreenSize) || b.DefectRatePercent is not null
            ? new TraceSpecsDto(b.MoisturePercent, Opt(b.ScreenSize), b.DefectRatePercent)
            : null;

        var certifications = b.Certifications
            .OrderBy(c => c.SortOrder)
            .Select(c => new TraceCertificationDto(c.Name, Opt(c.Number)))
            .ToList();

        var hasAny = b.CuppingScore is not null || Has(b.CuppedBy) || flavours.Count > 0
            || sensory is not null || specs is not null || certifications.Count > 0 || Has(b.LabReportImageUrl);

        return hasAny
            ? new TraceQualityDto(
                b.CuppingScore, Opt(b.CuppedBy), flavours, sensory, specs, certifications, Opt(b.LabReportImageUrl))
            : null;
    }

    private static TraceBrewingDto? MapBrewing(RoastBatch b)
    {
        var guides = b.BrewGuides
            .OrderBy(g => g.SortOrder)
            .Select(g => new TraceBrewGuideDto(
                g.Method, Opt(g.Ratio), g.WaterTempCelsius, Opt(g.BrewTime), Opt(g.Note)))
            .ToList();

        return guides.Count > 0 || Has(b.StorageTip)
            ? new TraceBrewingDto(guides, Opt(b.StorageTip))
            : null;
    }

    private static bool Has(string value) => !string.IsNullOrWhiteSpace(value);

    private static string? Opt(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
