using Microsoft.Extensions.Options;
using NieFarm.Domain.Entities;

namespace NieFarm.Web.Services.Traceability;

/// <summary>
/// Section "Site". <see cref="PublicBaseUrl"/> is the address printed inside every batch QR code (R-26 D18). It is not
/// a secret: appsettings.json commits the production domain (https://niefarm.vn) as the default, appsettings.Development.json
/// overrides it with the local address, and any other environment (staging) must override it too.
/// </summary>
public sealed class SiteOptions
{
    public const string SectionName = "Site";

    public string PublicBaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// Builds absolute public URLs for printing. The base always comes from configuration and never from the request
/// host (a forged Host header, or a developer browsing on localhost, must not end up on a printed label). An empty or invalid value is fail-soft: one Error log per distinct bad value, <see cref="IsConfigured"/> is false, and only the
/// admin download features stop; the public pages are unaffected.
///
/// The path <c>/truy-xuat/{code}</c> and the domain are permanent once a label is printed: never rename the route
/// without keeping the old one as an alias.
/// </summary>
public sealed class PublicUrls(
    IOptionsMonitor<SiteOptions> monitor,
    ILogger<PublicUrls> logger)
{
    public const string TracePath = "/truy-xuat";

    private readonly object _gate = new();
    private string? _loggedFor;

    public bool IsConfigured => TryGetBaseUrl(out _);

    /// <summary>The validated base as <c>scheme://host[:port]</c> (no trailing slash).</summary>
    public bool TryGetBaseUrl(out Uri baseUri)
    {
        var raw = (monitor.CurrentValue.PublicBaseUrl ?? string.Empty).Trim();

        if (raw.Length == 0)
        {
            baseUri = null!;
            LogOnce(raw, "Site:PublicBaseUrl is not set, so QR codes and labels cannot be generated.");
            return false;
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || !IsAcceptable(uri))
        {
            baseUri = null!;
            LogOnce(raw,
                "Site:PublicBaseUrl is not valid: it must be an absolute https address (http only for localhost) " +
                "with a host and no user info, path, query or fragment.");
            return false;
        }

        baseUri = new Uri(uri.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
        return true;
    }

    /// <summary>The address a batch QR points at: <c>{base}/truy-xuat/{CODE}</c>. False for an invalid code.</summary>
    public bool TryGetTraceUrl(string code, out string url)
    {
        url = string.Empty;
        if (!RoastBatch.IsValidCode(code) || !TryGetBaseUrl(out var baseUri))
            return false;

        url = $"{baseUri.GetLeftPart(UriPartial.Authority)}{TracePath}/{code}";
        return true;
    }

    /// <summary>Short address a person can type, printed on the label: <c>host/truy-xuat</c>.</summary>
    public bool TryGetShortEntryText(out string text)
    {
        text = string.Empty;
        if (!TryGetBaseUrl(out var baseUri))
            return false;

        text = $"{baseUri.Authority}{TracePath}";
        return true;
    }

    private bool IsAcceptable(Uri uri)
    {
        var schemeOk = uri.Scheme == Uri.UriSchemeHttps
            || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback);

        return schemeOk
            && !string.IsNullOrEmpty(uri.Host)
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment)
            && (uri.AbsolutePath == "/" || uri.AbsolutePath.Length == 0);
    }

    private void LogOnce(string value, string message)
    {
        lock (_gate)
        {
            if (_loggedFor == value)
                return;

            _loggedFor = value;
        }

        logger.LogError("{Message}", message);
    }
}
