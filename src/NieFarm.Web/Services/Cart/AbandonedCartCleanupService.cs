using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using NieFarm.Application.Features.Carts;
using NieFarm.Application.Features.Carts.Commands;

namespace NieFarm.Web.Services.Cart;

/// <summary>
/// Background loop that periodically deletes abandoned guest carts (R-23) by sending
/// <see cref="PurgeAbandonedCartsCommand"/>. Every run gets its own DI scope, so it never touches a
/// circuit's scoped DbContext. Options are read from <see cref="IOptionsMonitor{T}"/> on every cycle.
/// Every exception is caught inside the loop: since .NET 8 an exception escaping ExecuteAsync stops the
/// whole host. Shutdown (OperationCanceledException) leaves silently. No cart id, anonymous id or coupon
/// code is ever logged. There is no cross-instance lock: the delete is idempotent and re-checks its
/// conditions, so two instances overlapping only waste work.
/// </summary>
public sealed class AbandonedCartCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<CartCleanupOptions> monitor,
    ILogger<AbandonedCartCleanupService> logger) : BackgroundService
{
    internal readonly record struct Settings(
        bool Enabled,
        TimeSpan Interval,
        int EmptyCartIdleDays,
        int IdleCartDays,
        int BatchSize,
        int MaxBatchesPerRun,
        TimeSpan InitialDelay);

    private readonly object _gate = new();
    private CartCleanupOptions? _resolvedFor;
    private Settings _resolved;
    private bool _disabledLogged;
    private Settings? _lastGood;
    private bool _readFailureLogged;
    private bool _readFailed;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var first = ReadSettings();
            await Task.Delay(first.InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var settings = ReadSettings();

                try
                {
                    if (settings.Enabled)
                    {
                        _disabledLogged = false;
                        await RunOnceAsync(settings, stoppingToken);
                    }
                    else if (!_readFailed && !_disabledLogged)
                    {
                        // Only the genuine switch is reported here; a paused run caused by an unreadable
                        // configuration was already reported by ReadSettings (one Warning per failure streak).
                        _disabledLogged = true;
                        logger.LogInformation("Guest cart cleanup is disabled (CartCleanup:Enabled = false).");
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Guest cart cleanup run failed; it will be retried on the next cycle.");
                }

                // Read again so an interval change applies from the next wait.
                await Task.Delay(ReadSettings().Interval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down.
        }
        catch (Exception ex)
        {
            // Never let anything escape: it would stop the host.
            logger.LogError(ex, "Guest cart cleanup service stopped unexpectedly.");
        }
    }

    /// <summary>
    /// Reads the current options and resolves them. Reading can itself throw (the configuration binder
    /// rejects a value of the wrong type, e.g. BatchSize=abc, and the whole section is lost, including a
    /// correct Enabled=false). A failure is caught here, logged as ONE Warning per failure streak, and
    /// fails CLOSED: the service keeps looping (last good, or default, interval) but deletes nothing until the
    /// configuration can be read again, so the Enabled=false switch can never be defeated by a typo
    /// elsewhere in the section. A bad configuration never stops the service. Cancellation is not involved here.
    /// </summary>
    private Settings ReadSettings()
    {
        try
        {
            var settings = Resolve(monitor.CurrentValue);
            _lastGood = settings;
            _readFailureLogged = false;
            _readFailed = false;
            return settings;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!_readFailureLogged)
            {
                _readFailureLogged = true;
                logger.LogWarning(ex,
                    "CartCleanup options could not be read; guest cart cleanup is paused until the configuration is fixed.");
            }

            _readFailed = true;
            return (_lastGood ?? DefaultSettings()) with { Enabled = false };
        }
    }

    private static Settings DefaultSettings()
    {
        var d = new CartCleanupOptions();
        return new Settings(
            d.Enabled,
            TimeSpan.FromMinutes(d.IntervalMinutes),
            CartRetentionPolicy.DefaultEmptyCartIdleDays,
            0,
            d.BatchSize,
            d.MaxBatchesPerRun,
            TimeSpan.FromSeconds(d.InitialDelaySeconds + Random.Shared.Next(0, d.InitialDelayJitterSeconds + 1)));
    }

    internal async Task RunOnceAsync(Settings settings, CancellationToken cancellationToken)
    {
        var started = DateTime.UtcNow;

        await using var scope = scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new PurgeAbandonedCartsCommand(
                settings.EmptyCartIdleDays,
                settings.IdleCartDays,
                settings.BatchSize,
                settings.MaxBatchesPerRun),
            cancellationToken);

        var elapsedMs = (long)(DateTime.UtcNow - started).TotalMilliseconds;

        if (!result.IsSuccess)
        {
            logger.LogWarning("Guest cart cleanup was rejected: {Status}.", result.Status);
            return;
        }

        var purged = result.Value;
        if (purged.CartsDeleted > 0)
            logger.LogInformation(
                "Guest cart cleanup removed {Carts} carts ({Items} items) in {Batches} batches, {ElapsedMs} ms; more remaining: {MoreRemaining}",
                purged.CartsDeleted, purged.ItemsDeleted, purged.Batches, elapsedMs, purged.MoreRemaining);
        else
            logger.LogDebug("Guest cart cleanup removed nothing ({ElapsedMs} ms).", elapsedMs);
    }

    /// <summary>
    /// Validates the options once per options instance (compared by reference, like
    /// SePayWebhookAuthenticator): a value out of range is replaced by its default and one Warning is
    /// logged. The initial delay is computed once per instance so a repeated call returns the same value.
    /// </summary>
    internal Settings Resolve(CartCleanupOptions options)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_resolvedFor, options))
                return _resolved;

            var defaults = new CartCleanupOptions();

            var interval = InRange(nameof(options.IntervalMinutes), options.IntervalMinutes, 60, 10080, defaults.IntervalMinutes);
            var emptyDays = InRange(nameof(options.EmptyCartIdleDays), options.EmptyCartIdleDays, 1, 60, CartRetentionPolicy.DefaultEmptyCartIdleDays);
            var batchSize = InRange(nameof(options.BatchSize), options.BatchSize, 50, 1000, defaults.BatchSize);
            var maxBatches = InRange(nameof(options.MaxBatchesPerRun), options.MaxBatchesPerRun, 1, 100, defaults.MaxBatchesPerRun);
            var delay = InRange(nameof(options.InitialDelaySeconds), options.InitialDelaySeconds, 0, 3600, defaults.InitialDelaySeconds);
            var jitter = InRange(nameof(options.InitialDelayJitterSeconds), options.InitialDelayJitterSeconds, 0, 3600, defaults.InitialDelayJitterSeconds);

            var idleDays = options.IdleCartDays;
            if (idleDays != 0 &&
                (idleDays < CartRetentionPolicy.MinIdleCartDays || idleDays > CartRetentionPolicy.MaxIdleCartDays))
            {
                logger.LogWarning(
                    "CartCleanup:IdleCartDays must be 0 (off) or between {Min} and {Max}; got {Value}. Rule C stays off.",
                    CartRetentionPolicy.MinIdleCartDays, CartRetentionPolicy.MaxIdleCartDays, idleDays);
                idleDays = 0;
            }

            var initialDelay = TimeSpan.FromSeconds(delay + (jitter > 0 ? Random.Shared.Next(0, jitter + 1) : 0));

            _resolved = new Settings(
                options.Enabled,
                TimeSpan.FromMinutes(interval),
                emptyDays,
                idleDays,
                batchSize,
                maxBatches,
                initialDelay);
            _resolvedFor = options;
            return _resolved;
        }
    }

    private int InRange(string name, int value, int min, int max, int fallback)
    {
        if (value >= min && value <= max)
            return value;

        logger.LogWarning(
            "CartCleanup:{Name} must be between {Min} and {Max}; got {Value}. Using {Default}.",
            name, min, max, value, fallback);
        return fallback;
    }
}
