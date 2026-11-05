using NieFarm.Application.Common.Images;

namespace NieFarm.Application.Features.Admin.Testimonials.Dtos;

public static class TestimonialImageLimits
{
    public const long MaxImageBytes = ManagedImages.MaxImageBytes;
    public const string Subfolder = "testimonials";

    public static readonly string[] AllowedContentTypes = ManagedImages.AllowedContentTypes;

    /// <summary>True only for photos uploaded through this feature. Seed rows have no photo.</summary>
    public static bool IsManagedImage(string? url) => ManagedImages.IsManagedImage(url, Subfolder);
}
