using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace NieFarm.Web.Services.Http;

/// <summary>
/// Section "ReverseProxy" (R-26 D17): which proxies may tell us the real client address. Off by default, so
/// development and existing behaviour are unchanged. When on, only the listed networks are trusted: trusting
/// everything (0.0.0.0/0) would let any caller forge X-Forwarded-For and walk around the per-IP rate limit, so such
/// an entry is refused. Only X-Forwarded-For / X-Forwarded-Proto are read (never CF-Connecting-IP separately).
/// </summary>
public sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    public bool Enabled { get; set; }

    /// <summary>CIDR ranges of the proxies in front of the app, for example the Cloudflare ranges.</summary>
    public string[] TrustedNetworks { get; set; } = [];

    /// <summary>How many proxies to unwind from the right of X-Forwarded-For (1 = one proxy, e.g. Cloudflare).</summary>
    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Builds the middleware options, or null when forwarding is off or nothing valid is trusted. Problems are
    /// reported through <paramref name="warn"/>, never thrown: a typo must not stop the site starting.
    /// </summary>
    public ForwardedHeadersOptions? ToForwardedHeadersOptions(Action<string> warn)
    {
        if (!Enabled)
            return null;

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = ForwardLimit is >= 1 and <= 10 ? ForwardLimit : 1
        };

        // Replace the defaults (loopback) with exactly what is configured.
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        var trusted = 0;
        foreach (var raw in TrustedNetworks ?? [])
        {
            if (!System.Net.IPNetwork.TryParse(raw?.Trim() ?? string.Empty, out var network))
            {
                warn("ReverseProxy:TrustedNetworks contains an entry that is not a valid CIDR range; it was ignored.");
                continue;
            }

            if (network.PrefixLength == 0)
            {
                warn("ReverseProxy:TrustedNetworks contains a range that matches every address; it was ignored " +
                     "because trusting everyone would let a caller forge X-Forwarded-For.");
                continue;
            }

            options.KnownIPNetworks.Add(network);
            trusted++;
        }

        if (trusted == 0)
        {
            warn("ReverseProxy is enabled but no valid trusted network is configured, so forwarded headers are ignored.");
            return null;
        }

        return options;
    }
}
