using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using NieFarm.Web.Services.Http;

namespace NieFarm.Web.Endpoints;

/// <summary>
/// The "traceability" rate-limit policy for the public pages behind the QR code (R-26 D16): a fixed window per
/// client key, no queue. Every request that reaches the pages is counted, found or not, because the
/// <c>{Code}</c> route matches any segment. The pages carry <c>[EnableRateLimiting(Policy)]</c> as endpoint
/// metadata, the same way <c>[Authorize]</c> works. A rejection is never logged individually.
/// </summary>
public static class TraceabilityRateLimit
{
    public const string Policy = "traceability";

    public static RateLimiterOptions AddTraceabilityPolicy(this RateLimiterOptions options, int permitPerMinute)
    {
        options.AddPolicy(Policy, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                ClientIpKey.For(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        return options;
    }

    /// <summary>True when the rejected request was aimed at an endpoint using this policy.</summary>
    public static bool IsTraceabilityRequest(HttpContext httpContext) =>
        httpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == Policy;

    /// <summary>
    /// A short Vietnamese HTML answer for a person who scanned too often, with Retry-After and no caching. Status
    /// code pages are switched off for this response (they would re-execute it through /not-found).
    /// </summary>
    public static async Task WriteRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var http = context.HttpContext;
        http.Features.Get<IStatusCodePagesFeature>()?.Enabled = false;

        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        http.Response.ContentType = "text/html; charset=utf-8";
        http.Response.Headers.CacheControl = "no-store";
        http.Response.Headers["X-Robots-Tag"] = "noindex";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            http.Response.Headers.RetryAfter =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        const string html = """
            <!DOCTYPE html>
            <html lang="vi">
            <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <meta name="robots" content="noindex, nofollow" />
            <title>Vui lòng thử lại sau - Nie Farm Việt Nam</title>
            </head>
            <body style="font-family:'Be Vietnam Pro',system-ui,sans-serif;max-width:32rem;margin:20vh auto;padding:0 1.5rem;text-align:center;color:#1c1c1c">
            <h1 style="font-size:1.4rem">Bạn thao tác quá nhanh</h1>
            <p>Bạn thao tác quá nhanh, vui lòng thử lại sau ít phút.</p>
            <p><a href="/">Về trang chủ NieFarm</a></p>
            </body>
            </html>
            """;

        await http.Response.WriteAsync(html, cancellationToken);
    }
}
