namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Minimal, process-local anti-guessing limiter for the promo-code box: counts failed applies per cart
/// owner key ("u:{userId}" or "a:{anonymousId}"). While locked out, every apply is refused, even a
/// valid code. IP/global limiting is left to R-21.
/// </summary>
public interface ICouponAttemptThrottle
{
    bool IsLockedOut(string ownerKey);
    void RecordFailure(string ownerKey);
}
