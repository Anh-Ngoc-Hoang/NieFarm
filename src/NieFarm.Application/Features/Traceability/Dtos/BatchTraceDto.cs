using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Traceability.Dtos;

/// <summary>
/// What the public traceability page shows for one roast batch. Every string is <c>string?</c> where null means
/// "the admin did not fill this in, hide it", and every optional block is null when all its fields are empty
/// (R-26 D8): the Application decides what is hidden, the page only checks <c>is not null</c>.
/// Deliberately carries nothing admin-only (no internal note, no pack count, no scan numbers) and no user id.
/// </summary>
/// <param name="BatchId">Never rendered; only used to count a scan.</param>
public sealed record BatchTraceDto(
    int BatchId,
    bool IsPublished,
    string Code,
    string CodeDisplay,
    TraceProductDto Product,
    TraceVerificationDto Verification,
    TraceOriginDto? Origin,
    TraceHarvestStepDto? Harvest,
    TraceProcessingStepDto? Processing,
    TraceRoastStepDto Roast,
    TracePackingStepDto? Packing,
    TraceQualityDto? Quality,
    TraceBrewingDto? Brewing);

/// <param name="Slug">Null when the product is hidden: the page then shows no link and no "buy again".</param>
public sealed record TraceProductDto(string Name, string? Slug, string? ImageUrl, string ImageAlt);

/// <param name="PeakFrom">Roast date plus the "peak from" days, already computed.</param>
public sealed record TraceVerificationDto(
    DateOnly RoastDate,
    DateOnly? BestBefore,
    DateOnly? PeakFrom,
    DateOnly? PeakTo,
    int? NetWeightGrams);

public sealed record TraceOriginDto(
    string? Province,
    string? Commune,
    string? Locality,
    int? AltitudeMin,
    int? AltitudeMax,
    string? Variety,
    string? FarmName,
    IReadOnlyList<string> FarmStoryParagraphs,
    string? FarmImageUrl);

public sealed record TraceHarvestStepDto(string? Season, DateOnly? From, DateOnly? To, string? Method);

public sealed record TraceProcessingStepDto(
    CoffeeProcessingMethod? Method,
    string? Note,
    int? FermentationHours,
    int? DryingDays,
    string? DryingMethod);

/// <summary>Always present: the roast date is required.</summary>
public sealed record TraceRoastStepDto(DateOnly RoastDate, RoastLevel? Level, string? RoasterName, string? Note);

public sealed record TracePackingStepDto(
    DateOnly? PackedOn,
    bool HasOneWayValve,
    bool IsNitrogenFlushed,
    string? Note);

public sealed record TraceQualityDto(
    decimal? CuppingScore,
    string? CuppedBy,
    IReadOnlyList<string> FlavourNotes,
    TraceSensoryDto? Sensory,
    TraceSpecsDto? Specs,
    IReadOnlyList<TraceCertificationDto> Certifications,
    string? LabReportImageUrl);

/// <summary>Each level is 1 to 5, or null when not rated.</summary>
public sealed record TraceSensoryDto(byte? Acidity, byte? Body, byte? Sweetness, byte? Bitterness);

public sealed record TraceSpecsDto(decimal? MoisturePercent, string? ScreenSize, decimal? DefectRatePercent);

public sealed record TraceCertificationDto(string Name, string? Number);

public sealed record TraceBrewingDto(IReadOnlyList<TraceBrewGuideDto> Guides, string? StorageTip);

public sealed record TraceBrewGuideDto(
    BrewMethod Method,
    string? Ratio,
    int? WaterTempCelsius,
    string? BrewTime,
    string? Note);
