using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Services;

/// <summary>
/// Stores admin uploads under wwwroot/images/{subfolder}. Deletes are best-effort and
/// every path is verified to resolve inside wwwroot/images before anything is removed.
/// </summary>
public class LocalFileImageStorage(IWebHostEnvironment env, ILogger<LocalFileImageStorage> logger)
    : IImageStorage
{
    public async Task<string> SaveAsync(
        Stream stream, string fileName, string subfolder, CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(env.WebRootPath, "images", subfolder);
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, uniqueName);

        await using var file = File.Create(filePath);
        await stream.CopyToAsync(file, cancellationToken);

        return $"/images/{subfolder}/{uniqueName}";
    }

    public Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        if (!TryResolveExistingFile(relativeUrl, out var fullPath))
            return Task.CompletedTask;

        try
        {
            File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete image file {FullPath}", fullPath);
        }

        return Task.CompletedTask;
    }

    public async Task DeleteManyAsync(
        IEnumerable<string> relativeUrls, CancellationToken cancellationToken = default)
    {
        if (relativeUrls is null)
            return;

        foreach (var url in relativeUrls)
            await DeleteAsync(url, cancellationToken);
    }

    /// <summary>
    /// Normalises a relative image URL and verifies it resolves to an existing file under
    /// wwwroot/images. Anything escaping that root is rejected and logged.
    /// </summary>
    private bool TryResolveExistingFile(string relativeUrl, out string fullPath)
    {
        fullPath = string.Empty;

        if (string.IsNullOrWhiteSpace(relativeUrl))
            return false;

        var normalized = relativeUrl.Trim();
        if (normalized.StartsWith('~'))
            normalized = normalized[1..];
        normalized = normalized.TrimStart('/', '\\');

        if (normalized.Contains(':')
            || Path.IsPathRooted(normalized)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || normalized.StartsWith(@"\\", StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected image path outside the allowed root: {RelativeUrl}", relativeUrl);
            return false;
        }

        var imagesRoot = Path.GetFullPath(Path.Combine(env.WebRootPath, "images"));
        var candidate = Path.GetFullPath(
            Path.Combine(env.WebRootPath, normalized.Replace('/', Path.DirectorySeparatorChar)));

        if (!candidate.StartsWith(imagesRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Rejected image path outside the allowed root: {RelativeUrl}", relativeUrl);
            return false;
        }

        if (!File.Exists(candidate))
            return false;

        fullPath = candidate;
        return true;
    }
}
