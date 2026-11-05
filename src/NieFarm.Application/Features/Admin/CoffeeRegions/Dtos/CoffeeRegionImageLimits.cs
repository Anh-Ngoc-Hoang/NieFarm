using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;

public static class CoffeeRegionImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "coffee-regions";

    public static readonly string[] AllowedContentTypes = ManagedImages.AllowedContentTypes;

    /// <summary>
    /// True only for images uploaded through this feature. The seeded region photos
    /// (quy-trinh-1/-2/-6.jpg) live at the top level of wwwroot/images, not under
    /// wwwroot/images/coffee-regions/, so they are never deleted by an admin action.
    /// </summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
