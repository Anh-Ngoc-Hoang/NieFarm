namespace NieFarm.Application.Common.Images;

public static class ManagedImages
{
    public const long MaxImageBytes = 5 * 1024 * 1024;

    public static readonly string[] AllowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif"
    ];

    /// <summary>
    /// True only for images uploaded through a feature's own subfolder (wwwroot/images/{subfolder}/).
    /// The design assets committed straight into wwwroot/images (shop-1.jpg, article-2.jpg, …)
    /// are deliberately excluded so an admin action can never delete them.
    /// </summary>
    public static bool IsManagedImage(string? url, string subfolder)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(subfolder))
            return false;

        var normalized = url.Replace('\\', '/').TrimStart('~', '/');
        return normalized.StartsWith($"images/{subfolder.Trim('/')}/", StringComparison.OrdinalIgnoreCase);
    }
}
