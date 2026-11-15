using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Services;

/// <summary>
/// Process-local failure counter for the promo-code box: 10 failures per cart owner within 15 minutes
/// locks that owner (every apply is refused, even a valid code). Resets on restart and is not shared
/// across instances; IP/global limiting is left to R-21. The window is fixed from the first failure
/// (the counter is created once and expires), like <see cref="OrderLookupThrottle"/>. The code that was
/// tried is never logged.
/// </summary>
internal sealed class CouponAttemptThrottle(IMemoryCache cache, ILogger<CouponAttemptThrottle> logger) : ICouponAttemptThrottle
{
    private const int MaxFailures = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private sealed class Counter { public int Count; }

    public bool IsLockedOut(string ownerKey)
        => cache.TryGetValue(Key(ownerKey), out Counter? c) && c is not null && Volatile.Read(ref c.Count) >= MaxFailures;

    public void RecordFailure(string ownerKey)
    {
        var counter = cache.GetOrCreate(Key(ownerKey), e =>
        {
            e.AbsoluteExpirationRelativeToNow = Window;
            return new Counter();
        })!;

        if (Interlocked.Increment(ref counter.Count) == MaxFailures)
            logger.LogWarning("Coupon attempts locked out for a cart owner after {Failures} failures", MaxFailures);
    }

    private static string Key(string ownerKey) => $"coupon-attempts:{ownerKey}";
}
