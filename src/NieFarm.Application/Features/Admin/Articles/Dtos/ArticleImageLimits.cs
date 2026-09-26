using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.Articles.Dtos;

public static class ArticleImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "articles";

    public static readonly string[] AllowedContentTypes = ManagedImages.AllowedContentTypes;

    /// <summary>
    /// True only for images uploaded through this feature. The design assets shipped in
    /// wwwroot/images (blog-1.jpg, article-2.jpg, …) are never deleted by an admin action.
    /// </summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
