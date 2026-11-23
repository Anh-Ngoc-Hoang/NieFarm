namespace NieFarm.Web.Services.Cart;

/// <summary>
/// Section "CartCleanup": the periodic deletion of abandoned guest carts (R-23). No secrets, so the
/// defaults live in appsettings.json. A value outside its range falls back to the default (one Warning),
/// see AbandonedCartCleanupService.Resolve. Rules A and B are always on; rule C is off while
/// <see cref="IdleCartDays"/> is 0.
/// </summary>
public sealed class CartCleanupOptions
{
    public const string SectionName = "CartCleanup";

    public bool Enabled { get; set; } = true;

    /// <summary>Minutes between runs (60..10080).</summary>
    public int IntervalMinutes { get; set; } = 360;

    /// <summary>Rule B: an empty guest cart not modified for this many days is deleted (1..60).</summary>
    public int EmptyCartIdleDays { get; set; } = 7;

    /// <summary>Rule C: any guest cart not modified for this many days is deleted. 0 = off; otherwise 14..3650.</summary>
    public int IdleCartDays { get; set; }

    /// <summary>Carts deleted per statement (50..1000).</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>Most batches per run (1..100).</summary>
    public int MaxBatchesPerRun { get; set; } = 20;

    /// <summary>Wait after start-up before the first run, so migration and seeding finish first (0..3600).</summary>
    public int InitialDelaySeconds { get; set; } = 120;

    /// <summary>Random extra wait (0..N seconds) so several instances do not run together (0..3600).</summary>
    public int InitialDelayJitterSeconds { get; set; } = 300;
}
