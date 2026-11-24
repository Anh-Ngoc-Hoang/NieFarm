using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.RoastBatches.Dtos;

// Nested inputs of SaveRoastBatchCommand. The same records are the blocks of RoastBatchEditDto, so the form model
// maps one to the other without a second set of shapes. One record per tab of the admin form.

public record RoastBatchGeneralInput(
    DateOnly RoastDate,
    DateOnly? BestBefore,
    int? PeakFromDays,
    int? PeakToDays,
    int? NetWeightGrams,
    int? PackCount,
    string? InternalNote);

public record RoastBatchOriginInput(
    string? Province,
    string? Commune,
    string? Locality,
    int? AltitudeMinMeters,
    int? AltitudeMaxMeters,
    string? Variety,
    string? FarmName,
    string? FarmStory,
    string? FarmImageUrl,
    string? HarvestSeason,
    DateOnly? HarvestedFrom,
    DateOnly? HarvestedTo,
    string? HarvestMethod);

/// <summary>The "Process" tab: processing, roasting and packing.</summary>
public record RoastBatchProcessInput(
    CoffeeProcessingMethod? ProcessingMethod,
    string? ProcessingNote,
    int? FermentationHours,
    int? DryingDays,
    string? DryingMethod,
    RoastLevel? RoastLevel,
    string? RoasterName,
    string? RoastNote,
    DateOnly? PackedOn,
    bool HasOneWayValve,
    bool IsNitrogenFlushed,
    string? PackagingNote);

public record RoastBatchCertificationInput(string Name, string? Number);

public record RoastBatchQualityInput(
    decimal? CuppingScore,
    string? CuppedBy,
    string? FlavourNotes,
    byte? AcidityLevel,
    byte? BodyLevel,
    byte? SweetnessLevel,
    byte? BitternessLevel,
    decimal? MoisturePercent,
    string? ScreenSize,
    decimal? DefectRatePercent,
    string? LabReportImageUrl,
    IReadOnlyList<RoastBatchCertificationInput> Certifications);

public record RoastBatchBrewGuideInput(
    BrewMethod Method,
    string? Ratio,
    int? WaterTempCelsius,
    string? BrewTime,
    string? Note);

public record RoastBatchBrewingInput(
    string? StorageTip,
    IReadOnlyList<RoastBatchBrewGuideInput> Guides);
