namespace NieFarm.Application.Common.Interfaces;

public interface IImageStorage
{
    /// <summary>
    /// Saves a stream to wwwroot/images/{subfolder}/{guid}{ext} and returns the relative URL.
    /// </summary>
    Task<string> SaveAsync(Stream stream, string fileName, string subfolder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Best-effort deletion of a previously saved image, given the relative URL returned by
    /// SaveAsync (e.g. "images/products/{guid}.png"). Never throws: missing files, invalid
    /// paths and IO errors are ignored or logged. Paths resolving outside wwwroot/images are rejected.
    /// </summary>
    Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken = default);

    /// <summary>Best-effort deletion of several images. Never throws.</summary>
    Task DeleteManyAsync(IEnumerable<string> relativeUrls, CancellationToken cancellationToken = default);
}
