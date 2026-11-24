using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.RoastBatches.Dtos;

/// <summary>A row of the admin batch list. Scan numbers come from the separate counter table.</summary>
public record RoastBatchAdminListDto(
    int Id,
    string Code,
    string CodeDisplay,
    string ProductName,
    DateOnly RoastDate,
    bool IsPublished,
    int TotalScans,
    int TodayScans,
    DateTime? LastScanAtUtc,
    bool HasActiveAlert,
    ScanAlertReason? AlertReason);

/// <summary>Everything the admin form edits, including the admin-only fields. Never sent to the public page.</summary>
public record RoastBatchEditDto(
    int Id,
    string Code,
    string CodeDisplay,
    int ProductId,
    bool IsPublished,
    RoastBatchGeneralInput General,
    RoastBatchOriginInput Origin,
    RoastBatchProcessInput Process,
    RoastBatchQualityInput Quality,
    RoastBatchBrewingInput Brewing);

/// <summary>What the label needs. Available for an unpublished batch too, so labels can be printed before going public.</summary>
public record RoastBatchLabelDto(
    string Code,
    string CodeDisplay,
    string ProductName,
    DateOnly RoastDate,
    int? NetWeightGrams);
