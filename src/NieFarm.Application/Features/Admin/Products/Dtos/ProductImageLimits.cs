using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.Products.Dtos;

public static class ProductImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "products";
    public const int MaxImages = 10;

    public static readonly string[] AllowedContentTypes = ManagedImages.AllowedContentTypes;

    /// <summary>
    /// True only for images uploaded through this feature. The design assets shipped in
    /// wwwroot/images (shop-1.jpg, home-3.jpg, …) are never deleted by an admin action.
    /// </summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
