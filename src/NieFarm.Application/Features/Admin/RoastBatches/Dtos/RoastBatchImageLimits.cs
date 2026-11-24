using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.RoastBatches.Dtos;

public static class RoastBatchImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "batches";

    /// <summary>
    /// JPEG, PNG and WebP only, the same narrowing as partner logos: uploads are served same-origin from
    /// wwwroot/images/batches/, and an SVG opened directly runs script in the site origin. No GIF, no PDF.
    /// </summary>
    public static readonly string[] AllowedContentTypes = ["image/png", "image/webp", "image/jpeg"];

    /// <summary>True only for images uploaded through this feature (the farm photo and the lab-report photo).</summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
