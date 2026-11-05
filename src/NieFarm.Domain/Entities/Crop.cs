using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One harvest year's "New Crop" story — season window, headline, plain-text story, origin facts,
/// tasting notes, photo, and up to <see cref="MaxProducts"/> catalog products to "shop the crop".
/// Admins keep the whole history; exactly one row can be <see cref="IsCurrent"/> at a time (the
/// database backstop is a filtered unique index on IsCurrent, see CropConfiguration), and only
/// that one is shown on the home page under "New Crop {Year}".
///
/// <see cref="IsCurrent"/> is only ever changed through <see cref="MarkCurrent"/> /
/// <see cref="ClearCurrent"/> — an ordinary <see cref="Update"/> never touches it, so editing a
/// crop's copy can never accidentally demote or promote it. Callers needing to change which crop
/// is current go through SetCurrentCropCommand's two-step clear-then-mark sequence
/// (docs/plans/home-redesign-crop-testimonials.md D1) — never a bare MarkCurrent/ClearCurrent
/// pair in the same SaveChanges.
/// </summary>
public class Crop : BaseEntity
{
    public const int MinYear = 2000;
    public const int MaxYear = 2100;
    public const int MaxProducts = 4;

    public const int SeasonLabelMaxLength = 100;
    public const int HeadlineMaxLength = 200;
    public const int StoryMaxLength = 2000;
    public const int RegionMaxLength = 150;
    public const int AltitudeMaxLength = 60;
    public const int VarietiesMaxLength = 200;
    public const int ProcessMaxLength = 150;
    public const int TastingNotesMaxLength = 300;
    public const int ImageUrlMaxLength = 500;
    public const int ImageAltMaxLength = 300;

    private readonly List<CropProduct> _products = [];

    public int Year { get; private set; }
    public string SeasonLabel { get; private set; } = string.Empty;
    public string Headline { get; private set; } = string.Empty;

    /// <summary>Plain text; paragraphs are separated by a blank line (CropText.SplitParagraphs).</summary>
    public string Story { get; private set; } = string.Empty;

    public string Region { get; private set; } = string.Empty;

    /// <summary>A string, not a number, so ranges like "1.200–1.500m" are expressible.</summary>
    public string Altitude { get; private set; } = string.Empty;

    public string Varieties { get; private set; } = string.Empty;
    public string Process { get; private set; } = string.Empty;

    /// <summary>Normalised comma list (trimmed, blanks dropped, case-insensitive de-duped, re-joined with ", ").</summary>
    public string TastingNotes { get; private set; } = string.Empty;

    public string ImageUrl { get; private set; } = string.Empty;
    public string ImageAlt { get; private set; } = string.Empty;

    public bool IsCurrent { get; private set; }

    /// <summary>Up to <see cref="MaxProducts"/> catalog products, in display order.</summary>
    public IReadOnlyCollection<CropProduct> Products => _products.AsReadOnly();

    private Crop() { }

    public static Crop Create(
        int year, string seasonLabel, string headline, string story,
        string? region, string? altitude, string? varieties, string? process, string? tastingNotes,
        string imageUrl, string imageAlt)
    {
        var crop = new Crop();
        crop.Update(year, seasonLabel, headline, story, region, altitude, varieties, process,
            tastingNotes, imageUrl, imageAlt);
        return crop;
    }

    /// <summary>Never touches <see cref="IsCurrent"/> or <see cref="Products"/> — see MarkCurrent/ClearCurrent and SetProducts.</summary>
    public void Update(
        int year, string seasonLabel, string headline, string story,
        string? region, string? altitude, string? varieties, string? process, string? tastingNotes,
        string imageUrl, string imageAlt)
    {
        Year = year;
        SeasonLabel = seasonLabel.Trim();
        Headline = headline.Trim();
        Story = story.Trim();
        Region = (region ?? string.Empty).Trim();
        Altitude = (altitude ?? string.Empty).Trim();
        Varieties = (varieties ?? string.Empty).Trim();
        Process = (process ?? string.Empty).Trim();
        TastingNotes = NormalizeTastingNotes(tastingNotes);
        ImageUrl = imageUrl.Trim();
        ImageAlt = imageAlt.Trim();
        SetUpdatedAt();
    }

    /// <summary>
    /// Diffs against the current link set instead of clearing and rebuilding: removes rows whose
    /// ProductId is no longer listed, re-numbers SortOrder on rows that stay, and adds new rows —
    /// a delete-then-insert of the same (CropId, ProductId) would otherwise depend on EF statement
    /// ordering against the unique index on that pair.
    /// </summary>
    public void SetProducts(IReadOnlyList<int> productIds)
    {
        var distinct = new List<int>();
        var seen = new HashSet<int>();
        foreach (var id in productIds)
        {
            if (id <= 0)
                throw new InvalidOperationException("Product id must be positive.");

            if (seen.Add(id))
                distinct.Add(id);
        }

        if (distinct.Count > MaxProducts)
            throw new InvalidOperationException($"A crop cannot list more than {MaxProducts} products.");

        _products.RemoveAll(p => !distinct.Contains(p.ProductId));

        for (var i = 0; i < distinct.Count; i++)
        {
            var productId = distinct[i];
            var existing = _products.FirstOrDefault(p => p.ProductId == productId);
            if (existing is not null)
                existing.SetSortOrder(i);
            else
                _products.Add(new CropProduct(productId, i));
        }

        SetUpdatedAt();
    }

    public void MarkCurrent()
    {
        IsCurrent = true;
        SetUpdatedAt();
    }

    public void ClearCurrent()
    {
        IsCurrent = false;
        SetUpdatedAt();
    }

    private static string NormalizeTastingNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return string.Empty;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var raw in notes.Split(','))
        {
            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
                continue;

            if (seen.Add(trimmed))
                result.Add(trimmed);
        }

        return string.Join(", ", result);
    }
}
