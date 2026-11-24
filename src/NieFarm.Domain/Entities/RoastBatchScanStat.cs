using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Scan counters for one <see cref="RoastBatch"/>, kept in a row of their own (1:1 by <see cref="BatchId"/>,
/// no navigation from the batch). The admin saves a batch with a whole-row update that marks every column of
/// <c>RoastBatches</c> as modified, so counters stored there would be overwritten by a stale value the moment
/// an admin saved the form; a separate row is written only by the periodic flush and the alert acknowledgement.
///
/// The anomaly rule lives here (<see cref="Record"/>): after each flush two thresholds are checked and a
/// threshold raises an alert only on the flush that crosses it (before &lt; threshold &lt;= after).
/// </summary>
public class RoastBatchScanStat : BaseEntity
{
    public int BatchId { get; private set; }

    public int TotalScans { get; private set; }
    public DateTime? FirstScanAt { get; private set; }
    public DateTime? LastScanAt { get; private set; }

    /// <summary>The Vietnam-time day <see cref="DayScans"/> belongs to.</summary>
    public DateOnly? DayVn { get; private set; }
    public int DayScans { get; private set; }

    public int PeakDayScans { get; private set; }
    public DateOnly? PeakDayVn { get; private set; }

    public DateTime? AlertRaisedAt { get; private set; }
    public ScanAlertReason? AlertReason { get; private set; }
    public DateTime? AlertAcknowledgedAt { get; private set; }

    private RoastBatchScanStat() { }

    public static RoastBatchScanStat Create(int batchId)
    {
        if (batchId <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchId), "Batch id must be positive.");

        return new RoastBatchScanStat { BatchId = batchId };
    }

    /// <summary>An alert is open until the admin acknowledges it (or a later crossing re-raises it).</summary>
    public bool IsAlertActive =>
        AlertRaisedAt is not null && (AlertAcknowledgedAt is null || AlertAcknowledgedAt < AlertRaisedAt);

    /// <summary>
    /// Adds <paramref name="count"/> de-duplicated scans that happened on Vietnam-time day <paramref name="dayVn"/>.
    /// Moving to a later day resets the daily counter; data for an earlier day (a tally that arrives late) only
    /// adds to the total. <paramref name="dailyThreshold"/> 0 turns the daily rule off; <paramref name="packLimit"/>
    /// null turns the pack-count rule off. Returns the reason when this call crossed a threshold, else null.
    /// </summary>
    public ScanAlertReason? Record(
        int count, DateTime firstUtc, DateTime lastUtc, DateOnly dayVn, int dailyThreshold, int? packLimit)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Scan count must be positive.");

        var totalBefore = TotalScans;
        TotalScans = checked(TotalScans + count);

        if (FirstScanAt is null || firstUtc < FirstScanAt)
            FirstScanAt = firstUtc;
        if (LastScanAt is null || lastUtc > LastScanAt)
            LastScanAt = lastUtc;

        var dayBefore = 0;
        var countsForToday = true;
        if (DayVn is null || dayVn > DayVn)
        {
            DayVn = dayVn;
            DayScans = count;
        }
        else if (dayVn == DayVn)
        {
            dayBefore = DayScans;
            DayScans = checked(DayScans + count);
        }
        else
        {
            countsForToday = false;
        }

        if (countsForToday && DayScans > PeakDayScans)
        {
            PeakDayScans = DayScans;
            PeakDayVn = DayVn;
        }

        // The two crossings are independent. When one flush crosses both, both are recorded (Both): with an else-if the
        // pack-count crossing would be lost for good, because "before < limit" can never hold again afterwards.
        var reason = default(ScanAlertReason);
        if (countsForToday && dailyThreshold > 0 && dayBefore < dailyThreshold && DayScans >= dailyThreshold)
            reason |= ScanAlertReason.DailySpike;
        if (packLimit is > 0 && totalBefore < packLimit && TotalScans >= packLimit)
            reason |= ScanAlertReason.ExceedsPackCount;

        if (reason != default)
        {
            AlertRaisedAt = DateTime.UtcNow;
            AlertReason = reason;
        }

        SetUpdatedAt();
        return reason == default ? null : reason;
    }

    /// <summary>Closes the open alert. Writes only <see cref="AlertAcknowledgedAt"/>; counters are untouched.</summary>
    public void AcknowledgeAlert(DateTime utcNow)
    {
        AlertAcknowledgedAt = utcNow;
        SetUpdatedAt();
    }
}
