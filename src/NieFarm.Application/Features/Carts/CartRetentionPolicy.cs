namespace NieFarm.Application.Features.Carts;

/// <summary>
/// The single source for how long a guest cart can still be reached by its owner, and therefore
/// when the abandoned-cart cleanup (R-23) may delete it.
///
/// A guest cart is found through the <c>nf_cart</c> cookie. <c>CartCookieMiddleware</c> appends that
/// cookie only when the request has none, with a FIXED expiry of <see cref="GuestCookieLifetime"/>
/// from the moment it was issued - it is never renewed. A guest cart is always created after its
/// cookie was issued, so the cookie expires no later than <c>Cart.CreatedAt + GuestCookieLifetime</c>;
/// past that (plus <see cref="UnreachableGrace"/>) nobody can open the cart any more.
///
/// WARNING: if the cookie is ever changed to renew itself (sliding expiry, or re-issued on every
/// request), rule A of <c>AbandonedGuestCartsSpec</c> becomes wrong and would delete carts that are
/// still in use. In that case switch rule A from <c>CreatedAt</c> to <c>UpdatedAt</c> (or to a
/// last-seen timestamp) before changing the cookie. <c>CartCookie.Lifetime</c> reads
/// <see cref="GuestCookieLifetime"/> so the two can never drift apart.
/// </summary>
public static class CartRetentionPolicy
{
    /// <summary>Fixed lifetime of the nf_cart cookie, counted from when it is issued.</summary>
    public static readonly TimeSpan GuestCookieLifetime = TimeSpan.FromDays(60);

    /// <summary>Safety margin for browser clock skew and a circuit that stays open for a very long time.</summary>
    public static readonly TimeSpan UnreachableGrace = TimeSpan.FromDays(7);

    /// <summary>A guest cart older than this (by CreatedAt) can no longer be reached by anyone.</summary>
    public static TimeSpan UnreachableAfter => GuestCookieLifetime + UnreachableGrace;

    /// <summary>Default for rule B: an empty guest cart not modified for this many days is deleted.</summary>
    public const int DefaultEmptyCartIdleDays = 7;

    /// <summary>Rule C (idle guest carts that still hold items), when enabled, must be at least this many days.</summary>
    public const int MinIdleCartDays = 14;

    /// <summary>Upper bound for rule C (10 years); keeps "now - N days" far from DateTime/TimeSpan overflow.</summary>
    public const int MaxIdleCartDays = 3650;
}
