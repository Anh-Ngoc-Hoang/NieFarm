using NieFarm.Web.Services.Cart;

namespace NieFarm.Web.Middleware;

/// <summary>
/// Ensures every HTML GET carries an anonymous cart-owner cookie, issued before the response
/// starts — a Blazor component cannot write a cookie once interactive rendering has begun,
/// because the response has already started. Always stashes the value in HttpContext.Items so
/// App.razor can read it even on the very request the cookie was just appended (a freshly
/// appended cookie is not present in Request.Cookies on the same request).
///
/// The cookie is appended ONLY when the request has none, so its expiry is fixed from issue and
/// never renewed. The abandoned-cart cleanup (R-23, CartRetentionPolicy) relies on exactly that to
/// decide a guest cart can no longer be reached. Do not make the cookie sliding, or re-issue it on
/// every request, without first moving rule A of AbandonedGuestCartsSpec from CreatedAt to UpdatedAt.
/// </summary>
public class CartCookieMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await next(context);
            return;
        }

        var anonymousId = context.Request.Cookies[CartCookie.Name];

        if (string.IsNullOrWhiteSpace(anonymousId))
        {
            anonymousId = Guid.NewGuid().ToString("N");

            context.Response.Cookies.Append(CartCookie.Name, anonymousId, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.Add(CartCookie.Lifetime)
            });
        }

        context.Items[CartCookie.HttpContextItemKey] = anonymousId;

        await next(context);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/_blazor") || path.StartsWithSegments("/_framework") ||
            path.StartsWithSegments("/api") || path.StartsWithSegments("/admin"))
            return true;

        // Static assets (css/js/images/fonts/...) — anything with a file extension. Matters
        // more here than in OfficeCoffee because NieFarm serves static assets via
        // MapStaticAssets() endpoints, which still traverse the middleware pipeline.
        if (!string.IsNullOrEmpty(Path.GetExtension(path.Value)))
            return true;

        return false;
    }
}
