using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Traceability.Commands;

namespace NieFarm.Web.Services.Traceability;

/// <summary>
/// Writes the in-memory scan counters to the database on a timer (R-26 D10): one <see cref="RecordBatchScansCommand"/>
/// per chunk of at most 500 tallies, and only when something was counted, so database load does not grow with
/// traffic. Every flush has its own DI scope (never a circuit one). Every exception is caught inside the loop
/// (one escaping a BackgroundService stops the host); a failed write puts the tallies back for the next cycle.
/// On shutdown there is one last flush with a 5 second limit. Logs only counts: never a batch code, an address or a
/// user agent. The recorder is per process, so this assumes a single instance.
/// </summary>
public sealed class BatchScanFlushService(
    IServiceScopeFactory scopeFactory,
    BatchScanRecorder recorder,
    TraceabilitySettingsProvider settings,
    ILogger<BatchScanFlushService> logger) : BackgroundService
{
    private const int ChunkSize = RecordBatchScansCommandValidator.MaxTallies;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(settings.Current.FlushInterval, stoppingToken);
                await FlushAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down; StopAsync flushes once more.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scan flush service stopped unexpectedly.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        await FlushAsync(cts.Token);
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        List<BatchScanTally> pending;
        try
        {
            pending = recorder.Drain();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scan counters could not be drained.");
            return;
        }

        if (pending.Count == 0)
            return;

        var current = settings.Current;
        var written = 0;
        var dropped = 0;

        try
        {
            for (var offset = 0; offset < pending.Count; offset += ChunkSize)
            {
                var chunk = pending.GetRange(offset, Math.Min(ChunkSize, pending.Count - offset));

                await using var scope = scopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await sender.Send(
                    new RecordBatchScansCommand(chunk, current.DailyScanAlertThreshold, current.PackCountAlertMultiplier),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    written += chunk.Count;
                }
                else
                {
                    // A rejected command (not an exception) would be rejected again, so it is not retried.
                    dropped += chunk.Count;
                    logger.LogWarning(
                        "Scan flush of {Tallies} tallies was rejected ({Status}) and discarded.",
                        chunk.Count, result.Status);
                }
            }
        }
        catch (Exception ex)
        {
            // Put back everything not yet written (a chunk that threw may or may not have been saved; the
            // save is one transaction, so it is either all or nothing) and try again next cycle.
            recorder.Requeue(pending.Skip(written + dropped));
            logger.LogWarning(
                "Scan flush failed ({ExceptionType}); {Tallies} tallies were kept for the next cycle.",
                ex.GetType().Name, pending.Count - written - dropped);
        }
    }
}
