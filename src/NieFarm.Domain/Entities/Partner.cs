using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A strategic partner shown in the home page's scrolling partner band. Standalone — no FK to any
/// other aggregate, managed only through admin CRUD at /admin/partners. An empty
/// <see cref="LogoUrl"/> means "no logo" (the name is rendered as a text wordmark instead) and an
/// empty <see cref="WebsiteUrl"/> means "no link". There is no alt column: a logo's alt text is
/// always <see cref="Name"/>.
/// </summary>
public class Partner : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int LogoUrlMaxLength = 500;
    public const int WebsiteUrlMaxLength = 500;

    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional; <c>""</c> means no logo. Alt text is always <see cref="Name"/>.</summary>
    public string LogoUrl { get; private set; } = string.Empty;

    /// <summary>Optional absolute http(s) address; <c>""</c> means no link.</summary>
    public string WebsiteUrl { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; } = true;

    private Partner() { }

    public static Partner Create(string name, string logoUrl, string websiteUrl, int sortOrder, bool isVisible)
    {
        var partner = new Partner();
        partner.Update(name, logoUrl, websiteUrl, sortOrder, isVisible);
        return partner;
    }

    /// <summary>
    /// The validator enforces the same rules for friendly messages; these guards are the last
    /// line of defence against a malformed row actually being persisted.
    /// </summary>
    public void Update(string name, string logoUrl, string websiteUrl, int sortOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (!IsValidWebsiteUrl(websiteUrl))
            throw new ArgumentException(
                "Website must be a full address starting with http:// or https://.", nameof(websiteUrl));

        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");

        Name = name.Trim();
        LogoUrl = (logoUrl ?? string.Empty).Trim();
        WebsiteUrl = (websiteUrl ?? string.Empty).Trim();
        SortOrder = sortOrder;
        IsVisible = isVisible;
        SetUpdatedAt();
    }

    /// <summary>
    /// The single website rule, shared by the validator and <see cref="Update"/>: empty/whitespace,
    /// or an absolute http/https address with a host and no user-info (which blocks
    /// <c>https://trusted@evil.tld</c>). A missing scheme is rejected, never auto-prefixed.
    /// </summary>
    public static bool IsValidWebsiteUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrEmpty(uri.Host)
            && string.IsNullOrEmpty(uri.UserInfo);
    }
}
