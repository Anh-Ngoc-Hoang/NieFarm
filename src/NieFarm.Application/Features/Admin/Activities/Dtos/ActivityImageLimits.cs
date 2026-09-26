using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.Activities.Dtos;

public static class ActivityImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "activities";

    public static readonly string[] AllowedContentTypes = ManagedImages.AllowedContentTypes;

    /// <summary>
    /// True only for images uploaded through this feature. The committed design assets
    /// (activities-1.jpg … activities-6.jpg) live at the top level of wwwroot/images, not under
    /// wwwroot/images/activities/, so they are never deleted by an admin action.
    /// </summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
