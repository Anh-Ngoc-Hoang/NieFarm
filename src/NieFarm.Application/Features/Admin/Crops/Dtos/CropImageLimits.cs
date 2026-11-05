using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.Crops.Dtos;

public static class CropImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "crops";

    public static readonly string[] AllowedContentTypes = ManagedImages.AllowedContentTypes;

    /// <summary>
    /// True only for images uploaded through this feature. The seeded 2027 crop's image
    /// (quy-trinh-4.jpg) lives at the top level of wwwroot/images, not under
    /// wwwroot/images/crops/, so it is never deleted by an admin action.
    /// </summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
