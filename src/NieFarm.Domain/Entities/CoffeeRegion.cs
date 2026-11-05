using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One of the three fixed regions on the home page's "Vietnam Coffee Stories" map — its story
/// copy, four origin facts, an optional photo and up to <see cref="MaxAreas"/> well-known coffee
/// areas.
///
/// There is exactly one row per <see cref="CoffeeRegionKey"/>: the map's shapes are fixed in
/// source, so rows are only ever created by the seeder (<see cref="Create"/>), never added or
/// deleted by an admin. <see cref="UpdateContent"/> never touches <see cref="Key"/> or
/// <see cref="Areas"/>, and the fixed map label ("Tây Nguyên – Nam Bộ") is presentation in Web, not a column,
/// so an admin cannot make the map say something that disagrees with its shape.
///
/// Every text field is plain text (no MarkupString, no sanitiser); <see cref="Description"/>
/// separates paragraphs with a blank line.
/// </summary>
public class CoffeeRegion : BaseEntity
{
    public const int TitleMaxLength = 120;
    public const int TaglineMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int AltitudeMaxLength = 60;
    public const int VarietiesMaxLength = 200;
    public const int ClimateMaxLength = 200;
    public const int FlavorProfileMaxLength = 200;
    public const int ImageUrlMaxLength = 500;
    public const int ImageAltMaxLength = 300;
    public const int MaxAreas = 12;

    private readonly List<CoffeeRegionArea> _areas = [];

    public CoffeeRegionKey Key { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Tagline { get; private set; } = string.Empty;

    /// <summary>Plain text; paragraphs are separated by a blank line.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>A string, not a number, so ranges like "Khoảng 600–1.500 m" are expressible.</summary>
    public string Altitude { get; private set; } = string.Empty;

    public string Varieties { get; private set; } = string.Empty;
    public string Climate { get; private set; } = string.Empty;
    public string FlavorProfile { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public string ImageAlt { get; private set; } = string.Empty;

    /// <summary>Well-known coffee areas, in display order (<see cref="CoffeeRegionArea.SortOrder"/>).</summary>
    public IReadOnlyCollection<CoffeeRegionArea> Areas => _areas.AsReadOnly();

    private CoffeeRegion() { }

    /// <summary>Only the seeder calls this — there is no admin create path.</summary>
    public static CoffeeRegion Create(
        CoffeeRegionKey key, string title, string? tagline, string description,
        string? altitude, string? varieties, string? climate, string? flavorProfile,
        string? imageUrl, string? imageAlt)
    {
        if (!Enum.IsDefined(key))
            throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown coffee region.");

        var region = new CoffeeRegion { Key = key };
        region.UpdateContent(title, tagline, description, altitude, varieties, climate,
            flavorProfile, imageUrl, imageAlt);
        return region;
    }

    /// <summary>
    /// Replaces the editable copy. Trims every value (null becomes empty). Clears
    /// <see cref="ImageAlt"/> when there is no photo, so no stale alt text is stored. Never
    /// touches <see cref="Key"/> or <see cref="Areas"/> — see <see cref="SetAreas"/>.
    /// </summary>
    public void UpdateContent(
        string title, string? tagline, string description,
        string? altitude, string? varieties, string? climate, string? flavorProfile,
        string? imageUrl, string? imageAlt)
    {
        Title = Clean(title);
        Tagline = Clean(tagline);
        Description = Clean(description);
        Altitude = Clean(altitude);
        Varieties = Clean(varieties);
        Climate = Clean(climate);
        FlavorProfile = Clean(flavorProfile);
        ImageUrl = Clean(imageUrl);
        ImageAlt = ImageUrl.Length == 0 ? string.Empty : Clean(imageAlt);
        SetUpdatedAt();
    }

    /// <summary>
    /// Diffs against the current areas instead of clearing and rebuilding: a row with an
    /// existing Id is updated in place and re-numbered, a row with no Id is added, and an
    /// existing area that is not listed is removed. The list order becomes the display order.
    /// Throws on more than <see cref="MaxAreas"/> rows, an empty name, a duplicate name (trimmed,
    /// case-insensitive), or a duplicate or unknown Id.
    /// </summary>
    public void SetAreas(IReadOnlyList<(int? Id, string Name, string? Note)> areas)
    {
        if (areas.Count > MaxAreas)
            throw new InvalidOperationException($"A region cannot list more than {MaxAreas} areas.");

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ids = new HashSet<int>();

        foreach (var (id, name, _) in areas)
        {
            var trimmed = Clean(name);
            if (trimmed.Length == 0)
                throw new InvalidOperationException("Area name is required.");

            if (!names.Add(trimmed))
                throw new InvalidOperationException($"The area \"{trimmed}\" is listed twice.");

            if (id is { } existingId)
            {
                if (!ids.Add(existingId))
                    throw new InvalidOperationException("The same area is listed twice.");

                if (_areas.All(a => a.Id != existingId))
                    throw new InvalidOperationException($"Area {existingId} does not belong to this region.");
            }
        }

        _areas.RemoveAll(a => !ids.Contains(a.Id));

        for (var i = 0; i < areas.Count; i++)
        {
            var (id, name, note) = areas[i];
            if (id is { } existingId)
                _areas.First(a => a.Id == existingId).Update(Clean(name), Clean(note), i);
            else
                _areas.Add(new CoffeeRegionArea(Clean(name), Clean(note), i));
        }

        SetUpdatedAt();
    }

    private static string Clean(string? value) => (value ?? string.Empty).Trim();
}
