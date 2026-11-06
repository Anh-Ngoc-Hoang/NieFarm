using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Services;

/// <summary>
/// Process-local failure counter: 5 failures per phone or per code within 15 minutes locks that key.
/// Resets on restart and is not shared across instances; IP/global limiting is left to R-21.
/// </summary>
internal sealed class OrderLookupThrottle(IMemoryCache cache, ILogger<OrderLookupThrottle> logger) : IOrderLookupThrottle
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private sealed class Counter { public int Count; }

    public bool IsLockedOut(string code, string phone)
        => Count("phone", phone) >= MaxFailures || Count("code", code) >= MaxFailures;

    public void RecordFailure(string code, string phone)
    {
        Increment("phone", phone);
        Increment("code", code);
    }

    private int Count(string kind, string value)
        => cache.TryGetValue(Key(kind, value), out Counter? c) && c is not null ? Volatile.Read(ref c.Count) : 0;

    private void Increment(string kind, string value)
    {
        var counter = cache.GetOrCreate(Key(kind, value), e =>
        {
            e.AbsoluteExpirationRelativeToNow = Window;
            return new Counter();
        })!;

        if (Interlocked.Increment(ref counter.Count) == MaxFailures)
            logger.LogWarning("Order lookup locked out by {KeyKind}", kind);
    }

    private static string Key(string kind, string value) => $"order-lookup:{kind}:{value}";
}
