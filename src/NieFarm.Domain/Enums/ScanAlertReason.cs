namespace NieFarm.Domain.Enums;

/// <summary>Why a roast batch's scan counter raised an alert (R-26). Stored as int; never renumber.</summary>
[Flags]
public enum ScanAlertReason
{
    /// <summary>Distinct scans on one Vietnam-time day crossed the daily threshold.</summary>
    DailySpike = 1,

    /// <summary>Total scans crossed pack count x multiplier.</summary>
    ExceedsPackCount = 2,
    /// <summary>Both thresholds were crossed in the same flush (the value is the bitwise or of the two above).</summary>
    Both = DailySpike | ExceedsPackCount
}
