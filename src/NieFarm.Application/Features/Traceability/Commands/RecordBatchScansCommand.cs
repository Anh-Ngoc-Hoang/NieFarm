using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Traceability.Commands;

/// <summary>Distinct scans of one batch on one Vietnam-time day, as counted in memory by the web layer.</summary>
public record BatchScanTally(int BatchId, DateOnly DayVn, int Count, DateTime FirstAtUtc, DateTime LastAtUtc);

/// <summary>
/// Folds the in-memory scan tallies into <c>RoastBatchScanStats</c> in one small SaveChanges (R-26 D10). Sent by
/// the web layer background flush, never per request. The thresholds are the current configuration values.
/// </summary>
public record RecordBatchScansCommand(
    IReadOnlyList<BatchScanTally> Tallies,
    int DailyAlertThreshold,
    int PackCountAlertMultiplier) : IRequest<Result<BatchScanFlushResult>>;

/// <param name="Batches">Distinct batches written.</param>
/// <param name="Scans">Scans written.</param>
/// <param name="Dropped">Scans of batches that no longer exist (deleted since they were counted).</param>
/// <param name="AlertsRaised">Number of threshold crossings in this flush.</param>
public record BatchScanFlushResult(int Batches, int Scans, int Dropped, int AlertsRaised);
