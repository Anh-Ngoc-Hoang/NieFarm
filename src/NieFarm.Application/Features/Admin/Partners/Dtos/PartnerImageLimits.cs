using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.Partners.Dtos;

public static class PartnerImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "partners";

    /// <summary>
    /// PNG, WebP and JPEG only — deliberately narrower than ManagedImages.AllowedContentTypes.
    /// SVG is excluded because uploads are served same-origin from wwwroot/images/partners/, and an
    /// SVG opened directly runs script in the site's origin (stored XSS against signed-in admins);
    /// safe SVG would need a sanitiser dependency. GIF is excluded because an animated logo inside
    /// an already-moving marquee is doubly distracting. PNG and WebP both cover transparent logos.
    /// </summary>
    public static readonly string[] AllowedContentTypes = ["image/png", "image/webp", "image/jpeg"];

    /// <summary>
    /// True only for logos uploaded through this feature. The seed row has no logo; any future
    /// seeded logo must live at the top level of wwwroot/images so it is never admin-deletable.
    /// </summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
