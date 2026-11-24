using Microsoft.Extensions.Options;

namespace NieFarm.Web.Services.Traceability;

/// <summary>
/// Section "Traceability" (R-26 D10/D11/D13/D16). No secrets, so the defaults live in appsettings.json. A value
/// outside its range falls back to its default and one Warning is logged per options instance. Everything is read
/// again on each use except <see cref="RateLimitPerMinute"/>, which is fixed when the app starts.
/// </summary>
public sealed class TraceabilityOptions
{
    public const string SectionName = "Traceability";

    /// <summary>Requests per minute per client on the public traceability pages (5..600). Needs a restart.</summary>
    public int RateLimitPerMinute { get; set; } = 30;

    /// <summary>The same client scanning the same batch within this many minutes counts once (0..1440; 0 = off).</summary>
    public int ScanDedupeMinutes { get; set; } = 30;

    /// <summary>Seconds between writes of the in-memory scan counters to the database (15..3600).</summary>
    public int FlushIntervalSeconds { get; set; } = 120;

    /// <summary>Distinct scans of one batch in one Vietnam-time day that raise an alert (0 = off, or 1..100000).</summary>
    public int DailyScanAlertThreshold { get; set; } = 200;

    /// <summary>An alert is raised when total scans reach pack count times this (0 = off, or 1..100).</summary>
    public int PackCountAlertMultiplier { get; set; } = 3;
}

/// <summary>The validated, immutable form of <see cref="TraceabilityOptions"/>.</summary>
public sealed record TraceabilitySettings(
    int RateLimitPerMinute,
    int ScanDedupeMinutes,
    TimeSpan FlushInterval,
    int DailyScanAlertThreshold,
    int PackCountAlertMultiplier)
{
    /// <summary>
    /// Replaces each out-of-range value with its default. <paramref name="onInvalid"/> receives
    /// (name, value, allowed range, default used) so the caller can log it.
    /// </summary>
    public static TraceabilitySettings Resolve(
        TraceabilityOptions options, Action<string, int, string, int>? onInvalid = null)
    {
        var d = new TraceabilityOptions();

        int Pick(string name, int value, int min, int max, int fallback, bool allowZero = false)
        {
            if ((value >= min && value <= max) || (allowZero && value == 0))
                return value;

            onInvalid?.Invoke(name, value, allowZero ? $"0 or {min}..{max}" : $"{min}..{max}", fallback);
            return fallback;
        }

        return new TraceabilitySettings(
            Pick(nameof(options.RateLimitPerMinute), options.RateLimitPerMinute, 5, 600, d.RateLimitPerMinute),
            Pick(nameof(options.ScanDedupeMinutes), options.ScanDedupeMinutes, 0, 1440, d.ScanDedupeMinutes),
            TimeSpan.FromSeconds(Pick(
                nameof(options.FlushIntervalSeconds), options.FlushIntervalSeconds, 15, 3600, d.FlushIntervalSeconds)),
            Pick(nameof(options.DailyScanAlertThreshold), options.DailyScanAlertThreshold, 1, 100_000,
                d.DailyScanAlertThreshold, allowZero: true),
            Pick(nameof(options.PackCountAlertMultiplier), options.PackCountAlertMultiplier, 1, 100,
                d.PackCountAlertMultiplier, allowZero: true));
    }
}

/// <summary>
/// Singleton view of the current, validated settings. Validation (and its Warning) runs once per options instance,
/// compared by reference. A value of the wrong type makes the binder throw; that is caught here (one Warning per
/// failure streak) and the last good settings, or the defaults, stay in force.
/// </summary>
public sealed class TraceabilitySettingsProvider(
    IOptionsMonitor<TraceabilityOptions> monitor,
    ILogger<TraceabilitySettingsProvider> logger)
{
    private readonly object _gate = new();
    private TraceabilityOptions? _resolvedFor;
    private TraceabilitySettings _resolved = TraceabilitySettings.Resolve(new TraceabilityOptions());
    private bool _readFailureLogged;

    public TraceabilitySettings Current
    {
        get
        {
            try
            {
                var options = monitor.CurrentValue;
                lock (_gate)
                {
                    if (!ReferenceEquals(_resolvedFor, options))
                    {
                        _resolved = TraceabilitySettings.Resolve(options, (name, value, range, fallback) =>
                            logger.LogWarning(
                                "Traceability:{Name} must be {Range}; got {Value}. Using {Default}.",
                                name, range, value, fallback));
                        _resolvedFor = options;
                    }

                    _readFailureLogged = false;
                    return _resolved;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lock (_gate)
                {
                    if (!_readFailureLogged)
                    {
                        _readFailureLogged = true;
                        logger.LogWarning(ex,
                            "Traceability options could not be read; the last valid settings stay in force.");
                    }

                    return _resolved;
                }
            }
        }
    }
}
