namespace NieFarm.Web.Services.Traceability;

/// <summary>
/// Decides which requests are not a person scanning a pack (R-26 D11), so they never move a batch counter: link
/// previews, crawlers, scripts and browser prefetches. In-app browsers of Zalo, Facebook and so on are real
/// customers and are deliberately not filtered. An unknown bot still gets counted; that error is accepted.
/// </summary>
public static class ScanClientClassifier
{
    private const int MaxUserAgentLength = 512;

    private static readonly string[] BotMarkers =
    [
        "bot", "crawl", "spider", "slurp", "facebookexternalhit", "preview", "curl", "wget",
        "python", "go-http-client", "httpclient", "headless", "lighthouse"
    ];

    /// <summary>An empty or oversized user agent, or one containing a known automation marker.</summary>
    public static bool IsLikelyBot(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent) || userAgent.Length > MaxUserAgentLength)
            return true;

        foreach (var marker in BotMarkers)
        {
            if (userAgent.Contains(marker, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>A browser warming the page up (<c>Sec-Purpose: prefetch</c> or the older <c>Purpose</c>), not a visit.</summary>
    public static bool IsPrefetch(HttpRequest request) =>
        request.Headers["Sec-Purpose"].ToString().Contains("prefetch", StringComparison.OrdinalIgnoreCase)
        || request.Headers["Purpose"].ToString().Contains("prefetch", StringComparison.OrdinalIgnoreCase);
}
