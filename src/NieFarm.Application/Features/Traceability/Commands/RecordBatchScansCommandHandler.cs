using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Traceability.Commands;

/// <summary>
/// Runs in the background flush's own DI scope, so the write repository's context is its own and never a
/// circuit one. Tallies for batches deleted in the meantime are dropped (counted in <c>Dropped</c>), never an FK error.
/// </summary>
public class RecordBatchScansCommandHandler(
    IReadRepositoryBase<RoastBatch> batches,
    IRepositoryBase<RoastBatchScanStat> stats,
    ILogger<RecordBatchScansCommandHandler> logger)
    : IRequestHandler<RecordBatchScansCommand, Result<BatchScanFlushResult>>
{
    public async Task<Result<BatchScanFlushResult>> Handle(
        RecordBatchScansCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Tallies.Select(t => t.BatchId).Distinct().ToList();

        var infos = (await batches.ListAsync(new RoastBatchScanInfoSpec(ids), cancellationToken))
            .ToDictionary(i => i.Id);

        var kept = request.Tallies.Where(t => infos.ContainsKey(t.BatchId)).ToList();
        var dropped = request.Tallies.Where(t => !infos.ContainsKey(t.BatchId)).Sum(t => t.Count);

        if (kept.Count == 0)
            return Result.Success(new BatchScanFlushResult(0, 0, dropped, 0));

        var existing = (await stats.ListAsync(new ScanStatsByBatchIdsSpec(infos.Keys.ToList()), cancellationToken))
            .ToDictionary(s => s.BatchId);

        var created = new List<RoastBatchScanStat>();
        var raised = new List<(RoastBatchScanStat Stat, ScanAlertReason Reason)>();

        foreach (var tally in kept.OrderBy(t => t.DayVn).ThenBy(t => t.FirstAtUtc))
        {
            if (!existing.TryGetValue(tally.BatchId, out var stat))
            {
                stat = RoastBatchScanStat.Create(tally.BatchId);
                existing[tally.BatchId] = stat;
                created.Add(stat);
            }

            var reason = stat.Record(
                tally.Count, tally.FirstAtUtc, tally.LastAtUtc, tally.DayVn,
                request.DailyAlertThreshold,
                PackLimit(infos[tally.BatchId].PackCount, request.PackCountAlertMultiplier));

            if (reason is { } r)
                raised.Add((stat, r));
        }

        // New rows go in with the changes to the tracked ones in a single SaveChanges.
        if (created.Count > 0)
            await stats.AddRangeAsync(created, cancellationToken);
        else
            await stats.SaveChangesAsync(cancellationToken);

        // Only ids and counts: never the code, an address or a user agent.
        foreach (var (stat, reason) in raised)
        {
            logger.LogWarning(
                "Unusual scan activity on roast batch {BatchId}: {Reason}, {DayScans} scans today, {TotalScans} in total",
                stat.BatchId, reason, stat.DayScans, stat.TotalScans);
        }

        return Result.Success(new BatchScanFlushResult(
            kept.Select(t => t.BatchId).Distinct().Count(),
            kept.Sum(t => t.Count),
            dropped,
            raised.Count));
    }

    /// <summary>Pack count x multiplier, or null when either is missing (the rule is off).</summary>
    private static int? PackLimit(int? packCount, int multiplier)
    {
        if (packCount is not > 0 || multiplier <= 0)
            return null;

        var limit = (long)packCount.Value * multiplier;
        return (int)Math.Min(limit, int.MaxValue);
    }
}
