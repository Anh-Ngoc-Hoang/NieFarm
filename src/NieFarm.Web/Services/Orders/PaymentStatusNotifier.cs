using System.Collections.Concurrent;
using System.Collections.Immutable;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Domain.Enums;

namespace NieFarm.Web.Services.Orders;

/// <summary>
/// Singleton, in-memory fan-out of payment status changes to the pages that are open on an order.
/// A component may subscribe only AFTER it has proven the right to see that order (success page: the
/// code matches PlacedOrderState; lookup: after a verified lookup; my-order: the owner), so a guessed
/// code can never be used to listen in. The message is only code + status. Callbacks are fired without
/// being awaited, each in its own try/catch, so a payment webhook never waits on UI work. Single
/// process only: with several app instances an event reaches only circuits on the instance that
/// handled it (reloading always works).
/// </summary>
public sealed class PaymentStatusNotifier(ILogger<PaymentStatusNotifier> logger) : IPaymentStatusPublisher
{
    private sealed record Subscription(long Id, Func<PaymentStatus, Task> Callback);

    private readonly ConcurrentDictionary<string, ImmutableArray<Subscription>> _subscribers = new();
    private long _nextId;

    public IDisposable Subscribe(string orderCode, Func<PaymentStatus, Task> callback)
    {
        var key = Normalize(orderCode);
        var subscription = new Subscription(Interlocked.Increment(ref _nextId), callback);

        _subscribers.AddOrUpdate(
            key,
            _ => [subscription],
            (_, existing) => existing.Add(subscription));

        return new Handle(this, key, subscription.Id);
    }

    public Task PublishAsync(string orderCode, PaymentStatus status, CancellationToken ct)
    {
        if (_subscribers.TryGetValue(Normalize(orderCode), out var subscriptions))
        {
            // Suppress the caller's ExecutionContext (request AsyncLocals) and leave the publisher's
            // thread: callbacks run on the thread pool, never awaited, so the webhook never waits on UI work.
            using (ExecutionContext.SuppressFlow())
            {
                foreach (var subscription in subscriptions)
                {
                    var current = subscription;
                    _ = Task.Run(() => InvokeAsync(current, status), CancellationToken.None);
                }
            }
        }

        return Task.CompletedTask;
    }

    private async Task InvokeAsync(Subscription subscription, PaymentStatus status)
    {
        try
        {
            await subscription.Callback(status);
        }
        catch (Exception ex)
        {
            // A disposed circuit or a render failure is the subscriber's problem, never the payment's.
            logger.LogDebug(ex, "A payment status subscriber failed");
        }
    }

    private void Remove(string key, long id)
    {
        // Compare-and-swap loop so a concurrent Subscribe on the same code is never lost.
        while (_subscribers.TryGetValue(key, out var current))
        {
            var next = current.RemoveAll(s => s.Id == id);
            if (next.Length == current.Length)
                return;

            if (next.IsEmpty)
            {
                if (_subscribers.TryRemove(new KeyValuePair<string, ImmutableArray<Subscription>>(key, current)))
                    return;
            }
            else if (_subscribers.TryUpdate(key, next, current))
            {
                return;
            }
        }
    }

    private static string Normalize(string orderCode) => orderCode.Trim().ToUpperInvariant();

    private sealed class Handle(PaymentStatusNotifier owner, string key, long id) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                owner.Remove(key, id);
        }
    }
}
