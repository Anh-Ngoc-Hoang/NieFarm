using NieFarm.Application.Features.Carts;

namespace NieFarm.Web.Services.Cart;

/// <summary>Constants shared between CartCookieMiddleware, App.razor, CartState and
/// Pages/Account/Login.cshtml.cs.</summary>
public static class CartCookie
{
    public const string Name = "nf_cart";
    public const string HttpContextItemKey = "CartAnonymousId";

    /// <summary>
    /// Fixed lifetime of the cookie, counted from when it is issued. The value lives in
    /// <see cref="CartRetentionPolicy.GuestCookieLifetime"/> because the abandoned-cart cleanup (R-23)
    /// deletes guest carts once this cookie can no longer exist.
    ///
    /// WARNING: <c>CartCookieMiddleware</c> appends the cookie only when the request has none, so the
    /// expiry is never renewed. If you change that (sliding expiry, or re-issuing the cookie on every
    /// request), the cleanup would delete carts that are still in use: switch rule A of
    /// <c>AbandonedGuestCartsSpec</c> from CreatedAt to UpdatedAt (or a last-seen time) in the same change.
    /// </summary>
    public static readonly TimeSpan Lifetime = CartRetentionPolicy.GuestCookieLifetime;
}
