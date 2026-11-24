using System.Text;
using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>A certification line passed to <see cref="RoastBatch.SetCertifications"/>.</summary>
public readonly record struct CertificationSpec(string Name, string? Number);

/// <summary>A brew-guide line passed to <see cref="RoastBatch.SetBrewGuides"/>.</summary>
public readonly record struct BrewGuideSpec(
    BrewMethod Method, string? Ratio, int? WaterTempCelsius, string? BrewTime, string? Note);

/// <summary>
/// One roast batch of one <see cref="Product"/>, identified by a random public <see cref="Code"/> that is printed
/// as a QR on every pack of the batch (R-26). The page behind the QR shows only what the admin filled in: every
/// field other than the product, the roast date and the code is optional, and an unset text is stored as <c>""</c>
/// (like <see cref="Partner"/>). All free text is plain text (never rendered as markup) and stored in Unicode
/// normalisation form C, so the printed label can always draw the precomposed Vietnamese glyphs.
///
/// <see cref="Code"/> is immutable: it is printed. The domain does not generate it (that is the Application's
/// <c>BatchCode</c>); it only checks the shape. <see cref="InternalNote"/> and <see cref="PackCount"/> are
/// admin-only and never reach a public DTO. The validators give friendly messages; the guards here are the last
/// line of defence against a malformed row being persisted.
///
/// Scan counters live in <see cref="RoastBatchScanStat"/>, not here (see that class).
/// </summary>
public class RoastBatch : BaseEntity
{
    public const int CodeLength = 12;
    public const string CodeAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    public const string ImageUrlPrefix = "/images/batches/";

    public const int MaxCertifications = 6;
    public const int MaxBrewGuides = 6;

    public const int InternalNoteMaxLength = 500;
    public const int OriginProvinceMaxLength = 120;
    public const int OriginCommuneMaxLength = 120;
    public const int OriginLocalityMaxLength = 150;
    public const int VarietyMaxLength = 150;
    public const int FarmNameMaxLength = 150;
    public const int FarmStoryMaxLength = 2000;
    public const int ImageUrlMaxLength = 500;
    public const int HarvestSeasonMaxLength = 50;
    public const int HarvestMethodMaxLength = 150;
    public const int ProcessingNoteMaxLength = 200;
    public const int DryingMethodMaxLength = 150;
    public const int RoasterNameMaxLength = 100;
    public const int RoastNoteMaxLength = 200;
    public const int PackagingNoteMaxLength = 200;
    public const int CuppedByMaxLength = 100;
    public const int FlavourNotesMaxLength = 300;
    public const int ScreenSizeMaxLength = 30;
    public const int StorageTipMaxLength = 300;

    public const int MaxPeakDays = 730;
    public const int MaxNetWeightGrams = 50_000;
    public const int MaxPackCount = 1_000_000;
    public const int MaxAltitudeMeters = 3200;
    public const int MaxFermentationHours = 720;
    public const int MaxDryingDays = 120;

    private readonly List<RoastBatchCertification> _certifications = [];
    private readonly List<RoastBatchBrewGuide> _brewGuides = [];

    // Core
    public string Code { get; private set; } = string.Empty;
    public int ProductId { get; private set; }
    public Product? Product { get; private set; }
    public bool IsPublished { get; private set; }
    public DateOnly RoastDate { get; private set; }
    public DateOnly? BestBefore { get; private set; }
    public int? PeakFromDays { get; private set; }
    public int? PeakToDays { get; private set; }

    /// <summary>Leave unset when the batch is packed in several sizes.</summary>
    public int? NetWeightGrams { get; private set; }

    /// <summary>Admin only.</summary>
    public int? PackCount { get; private set; }

    /// <summary>Admin only.</summary>
    public string InternalNote { get; private set; } = string.Empty;

    // Origin
    public string OriginProvince { get; private set; } = string.Empty;
    public string OriginCommune { get; private set; } = string.Empty;
    public string OriginLocality { get; private set; } = string.Empty;
    public int? AltitudeMinMeters { get; private set; }
    public int? AltitudeMaxMeters { get; private set; }
    public string Variety { get; private set; } = string.Empty;
    public string FarmName { get; private set; } = string.Empty;

    /// <summary>Plain text; paragraphs separated by a blank line.</summary>
    public string FarmStory { get; private set; } = string.Empty;

    public string FarmImageUrl { get; private set; } = string.Empty;

    /// <summary>Free text such as "Vụ 2025/26", not a link to a Crop (a printed batch must not change when a crop is edited).</summary>
    public string HarvestSeason { get; private set; } = string.Empty;

    public DateOnly? HarvestedFrom { get; private set; }
    public DateOnly? HarvestedTo { get; private set; }
    public string HarvestMethod { get; private set; } = string.Empty;

    // Processing
    public CoffeeProcessingMethod? ProcessingMethod { get; private set; }
    public string ProcessingNote { get; private set; } = string.Empty;
    public int? FermentationHours { get; private set; }
    public int? DryingDays { get; private set; }
    public string DryingMethod { get; private set; } = string.Empty;

    // Roasting and packing
    public RoastLevel? RoastLevel { get; private set; }
    public string RoasterName { get; private set; } = string.Empty;
    public string RoastNote { get; private set; } = string.Empty;
    public DateOnly? PackedOn { get; private set; }
    public bool HasOneWayValve { get; private set; }
    public bool IsNitrogenFlushed { get; private set; }
    public string PackagingNote { get; private set; } = string.Empty;

    // Quality
    public decimal? CuppingScore { get; private set; }
    public string CuppedBy { get; private set; } = string.Empty;

    /// <summary>Normalised comma list (trimmed, blanks dropped, case-insensitive de-duped, re-joined with ", ").</summary>
    public string FlavourNotes { get; private set; } = string.Empty;

    public byte? AcidityLevel { get; private set; }
    public byte? BodyLevel { get; private set; }
    public byte? SweetnessLevel { get; private set; }
    public byte? BitternessLevel { get; private set; }
    public decimal? MoisturePercent { get; private set; }
    public string ScreenSize { get; private set; } = string.Empty;
    public decimal? DefectRatePercent { get; private set; }
    public string LabReportImageUrl { get; private set; } = string.Empty;

    // Brewing
    public string StorageTip { get; private set; } = string.Empty;

    public IReadOnlyCollection<RoastBatchCertification> Certifications => _certifications.AsReadOnly();
    public IReadOnlyCollection<RoastBatchBrewGuide> BrewGuides => _brewGuides.AsReadOnly();

    private RoastBatch() { }

    public static RoastBatch Create(string code, int productId, DateOnly roastDate)
    {
        if (!IsValidCode(code))
            throw new ArgumentException("Batch code has an invalid shape.", nameof(code));

        if (productId <= 0)
            throw new ArgumentOutOfRangeException(nameof(productId), "Product is required.");

        if (roastDate == default)
            throw new ArgumentException("Roast date is required.", nameof(roastDate));

        return new RoastBatch { Code = code, ProductId = productId, RoastDate = roastDate };
    }

    /// <summary>The batch alphabet drops 0, 1, I, L and O so a printed code is never misread.</summary>
    public static bool IsValidCode(string? code)
    {
        if (code is null || code.Length != CodeLength)
            return false;

        foreach (var c in code)
        {
            if (CodeAlphabet.IndexOf(c) < 0)
                return false;
        }

        return true;
    }

    public void UpdateGeneral(
        int productId, DateOnly roastDate, DateOnly? bestBefore, int? peakFromDays, int? peakToDays,
        int? netWeightGrams, int? packCount, string? internalNote)
    {
        if (productId <= 0)
            throw new ArgumentOutOfRangeException(nameof(productId), "Product is required.");

        if (roastDate == default)
            throw new ArgumentException("Roast date is required.", nameof(roastDate));

        if (bestBefore is { } bb && bb < roastDate)
            throw new ArgumentException("Best-before cannot be earlier than the roast date.", nameof(bestBefore));

        CheckRange(peakFromDays, 0, MaxPeakDays, nameof(peakFromDays));
        CheckRange(peakToDays, 0, MaxPeakDays, nameof(peakToDays));
        if (peakFromDays is { } pf && peakToDays is { } pt && pf > pt)
            throw new ArgumentException("Peak window start cannot be after its end.", nameof(peakFromDays));

        CheckRange(netWeightGrams, 1, MaxNetWeightGrams, nameof(netWeightGrams));
        CheckRange(packCount, 1, MaxPackCount, nameof(packCount));

        ProductId = productId;
        RoastDate = roastDate;
        BestBefore = bestBefore;
        PeakFromDays = peakFromDays;
        PeakToDays = peakToDays;
        NetWeightGrams = netWeightGrams;
        PackCount = packCount;
        InternalNote = Clean(internalNote, InternalNoteMaxLength, nameof(internalNote));
        SetUpdatedAt();
    }

    public void UpdateOrigin(
        string? province, string? commune, string? locality, int? altitudeMinMeters, int? altitudeMaxMeters,
        string? variety, string? farmName, string? farmStory, string? farmImageUrl,
        string? harvestSeason, DateOnly? harvestedFrom, DateOnly? harvestedTo, string? harvestMethod)
    {
        CheckRange(altitudeMinMeters, 0, MaxAltitudeMeters, nameof(altitudeMinMeters));
        CheckRange(altitudeMaxMeters, 0, MaxAltitudeMeters, nameof(altitudeMaxMeters));
        if (altitudeMinMeters is { } min && altitudeMaxMeters is { } max && min > max)
            throw new ArgumentException("Minimum altitude cannot exceed the maximum.", nameof(altitudeMinMeters));

        if (harvestedFrom is { } from && harvestedTo is { } to && from > to)
            throw new ArgumentException("Harvest start cannot be after its end.", nameof(harvestedFrom));

        if ((harvestedTo ?? harvestedFrom) is { } last && last > RoastDate)
            throw new ArgumentException("Harvest cannot end after the roast date.", nameof(harvestedTo));

        OriginProvince = Clean(province, OriginProvinceMaxLength, nameof(province));
        OriginCommune = Clean(commune, OriginCommuneMaxLength, nameof(commune));
        OriginLocality = Clean(locality, OriginLocalityMaxLength, nameof(locality));
        AltitudeMinMeters = altitudeMinMeters;
        AltitudeMaxMeters = altitudeMaxMeters;
        Variety = Clean(variety, VarietyMaxLength, nameof(variety));
        FarmName = Clean(farmName, FarmNameMaxLength, nameof(farmName));
        FarmStory = Clean(farmStory, FarmStoryMaxLength, nameof(farmStory));
        FarmImageUrl = CleanImage(farmImageUrl, nameof(farmImageUrl));
        HarvestSeason = Clean(harvestSeason, HarvestSeasonMaxLength, nameof(harvestSeason));
        HarvestedFrom = harvestedFrom;
        HarvestedTo = harvestedTo;
        HarvestMethod = Clean(harvestMethod, HarvestMethodMaxLength, nameof(harvestMethod));
        SetUpdatedAt();
    }

    public void UpdateProcessing(
        CoffeeProcessingMethod? method, string? note, int? fermentationHours, int? dryingDays, string? dryingMethod)
    {
        if (method is { } m && !Enum.IsDefined(m))
            throw new ArgumentException("Unknown processing method.", nameof(method));

        CheckRange(fermentationHours, 0, MaxFermentationHours, nameof(fermentationHours));
        CheckRange(dryingDays, 0, MaxDryingDays, nameof(dryingDays));

        ProcessingMethod = method;
        ProcessingNote = Clean(note, ProcessingNoteMaxLength, nameof(note));
        FermentationHours = fermentationHours;
        DryingDays = dryingDays;
        DryingMethod = Clean(dryingMethod, DryingMethodMaxLength, nameof(dryingMethod));
        SetUpdatedAt();
    }

    public void UpdateRoasting(
        RoastLevel? level, string? roasterName, string? roastNote, DateOnly? packedOn,
        bool hasOneWayValve, bool isNitrogenFlushed, string? packagingNote)
    {
        if (level is { } l && !Enum.IsDefined(l))
            throw new ArgumentException("Unknown roast level.", nameof(level));

        if (packedOn is { } packed && packed < RoastDate)
            throw new ArgumentException("Packing date cannot be earlier than the roast date.", nameof(packedOn));

        RoastLevel = level;
        RoasterName = Clean(roasterName, RoasterNameMaxLength, nameof(roasterName));
        RoastNote = Clean(roastNote, RoastNoteMaxLength, nameof(roastNote));
        PackedOn = packedOn;
        HasOneWayValve = hasOneWayValve;
        IsNitrogenFlushed = isNitrogenFlushed;
        PackagingNote = Clean(packagingNote, PackagingNoteMaxLength, nameof(packagingNote));
        SetUpdatedAt();
    }

    public void UpdateQuality(
        decimal? cuppingScore, string? cuppedBy, string? flavourNotes,
        byte? acidityLevel, byte? bodyLevel, byte? sweetnessLevel, byte? bitternessLevel,
        decimal? moisturePercent, string? screenSize, decimal? defectRatePercent, string? labReportImageUrl)
    {
        if (cuppingScore is { } cs && (cs <= 0m || cs > 100m))
            throw new ArgumentOutOfRangeException(nameof(cuppingScore), "Cupping score must be above 0 and at most 100.");

        CheckLevel(acidityLevel, nameof(acidityLevel));
        CheckLevel(bodyLevel, nameof(bodyLevel));
        CheckLevel(sweetnessLevel, nameof(sweetnessLevel));
        CheckLevel(bitternessLevel, nameof(bitternessLevel));

        if (moisturePercent is { } mp && (mp < 0m || mp > 20m))
            throw new ArgumentOutOfRangeException(nameof(moisturePercent), "Moisture must be between 0 and 20 percent.");

        if (defectRatePercent is { } dr && (dr < 0m || dr > 100m))
            throw new ArgumentOutOfRangeException(nameof(defectRatePercent), "Defect rate must be between 0 and 100 percent.");

        CuppingScore = cuppingScore;
        CuppedBy = Clean(cuppedBy, CuppedByMaxLength, nameof(cuppedBy));
        FlavourNotes = NormalizeFlavourNotes(flavourNotes);
        AcidityLevel = acidityLevel;
        BodyLevel = bodyLevel;
        SweetnessLevel = sweetnessLevel;
        BitternessLevel = bitternessLevel;
        MoisturePercent = moisturePercent;
        ScreenSize = Clean(screenSize, ScreenSizeMaxLength, nameof(screenSize));
        DefectRatePercent = defectRatePercent;
        LabReportImageUrl = CleanImage(labReportImageUrl, nameof(labReportImageUrl));
        SetUpdatedAt();
    }

    public void UpdateBrewing(string? storageTip)
    {
        StorageTip = Clean(storageTip, StorageTipMaxLength, nameof(storageTip));
        SetUpdatedAt();
    }

    /// <summary>Replaces the certification list (nothing else references these rows, so clear and rebuild).</summary>
    public void SetCertifications(IReadOnlyList<CertificationSpec> certifications)
    {
        if (certifications.Count > MaxCertifications)
            throw new InvalidOperationException($"A batch cannot list more than {MaxCertifications} certifications.");

        var rebuilt = new List<RoastBatchCertification>(certifications.Count);
        for (var i = 0; i < certifications.Count; i++)
        {
            var name = Clean(certifications[i].Name, RoastBatchCertification.NameMaxLength, "name");
            if (name.Length == 0)
                throw new ArgumentException("Certification name is required.", nameof(certifications));

            var number = Clean(certifications[i].Number, RoastBatchCertification.NumberMaxLength, "number");
            rebuilt.Add(new RoastBatchCertification(name, number, i));
        }

        _certifications.Clear();
        _certifications.AddRange(rebuilt);
        SetUpdatedAt();
    }

    /// <summary>
    /// Diffs by <see cref="BrewMethod"/> (unique per batch) instead of clearing and rebuilding: a delete-then-insert of
    /// the same (batch, method) would otherwise depend on EF statement ordering against the unique index.
    /// </summary>
    public void SetBrewGuides(IReadOnlyList<BrewGuideSpec> guides)
    {
        if (guides.Count > MaxBrewGuides)
            throw new InvalidOperationException($"A batch cannot list more than {MaxBrewGuides} brew guides.");

        var seen = new HashSet<BrewMethod>();
        foreach (var guide in guides)
        {
            if (!Enum.IsDefined(guide.Method))
                throw new ArgumentException("Unknown brew method.", nameof(guides));

            if (!seen.Add(guide.Method))
                throw new ArgumentException("A brew method can only be listed once.", nameof(guides));

            CheckRange(guide.WaterTempCelsius, 0, RoastBatchBrewGuide.MaxWaterTempCelsius, "waterTempCelsius");
        }

        _brewGuides.RemoveAll(g => !seen.Contains(g.Method));

        for (var i = 0; i < guides.Count; i++)
        {
            var spec = guides[i];
            var existing = _brewGuides.FirstOrDefault(g => g.Method == spec.Method);
            if (existing is null)
            {
                existing = new RoastBatchBrewGuide(spec.Method);
                _brewGuides.Add(existing);
            }

            existing.Set(
                Clean(spec.Ratio, RoastBatchBrewGuide.RatioMaxLength, "ratio"),
                spec.WaterTempCelsius,
                Clean(spec.BrewTime, RoastBatchBrewGuide.BrewTimeMaxLength, "brewTime"),
                Clean(spec.Note, RoastBatchBrewGuide.NoteMaxLength, "note"),
                i);
        }

        SetUpdatedAt();
    }

    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
        SetUpdatedAt();
    }

    /// <summary>Trim, NFC, <c>""</c> for null, and a length guard.</summary>
    private static string Clean(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value.Trim().Normalize(NormalizationForm.FormC);
        if (text.Length > maxLength)
            throw new ArgumentException($"Value is longer than {maxLength} characters.", paramName);

        return text;
    }

    private static string CleanImage(string? url, string paramName)
    {
        var text = Clean(url, ImageUrlMaxLength, paramName);
        if (!IsValidImageUrl(text))
            throw new ArgumentException($"Images must live under {ImageUrlPrefix}.", paramName);

        return text;
    }

    /// <summary>
    /// The one rule for a stored image URL, shared by the domain guard and the validator: empty, or under
    /// <see cref="ImageUrlPrefix"/> with no path escape (<c>..</c>), backslash, query, fragment or control character,
    /// so <c>/images/batches/../shop-1.jpg</c> is refused.
    /// </summary>
    public static bool IsValidImageUrl(string? url)
    {
        var text = url?.Trim() ?? string.Empty;
        if (text.Length == 0)
            return true;

        return text.StartsWith(ImageUrlPrefix, StringComparison.Ordinal)
            && !text.Contains("..", StringComparison.Ordinal)
            && !text.Any(c => c is '\\' or '?' or '#' || char.IsControl(c));
    }

    private static void CheckRange(int? value, int min, int max, string paramName)
    {
        if (value is { } v && (v < min || v > max))
            throw new ArgumentOutOfRangeException(paramName, $"Must be between {min} and {max}.");
    }

    private static void CheckLevel(byte? value, string paramName)
    {
        if (value is { } v && (v < 1 || v > 5))
            throw new ArgumentOutOfRangeException(paramName, "Must be between 1 and 5.");
    }

    /// <summary>
    /// The stored form of the flavour-notes list: trimmed, NFC, de-duplicated (case-insensitive) and rejoined with
    /// ", ". The validator measures THIS length (not the raw text), so it can never accept what the domain refuses:
    /// "a,b" becomes "a, b", which is longer than the raw input.
    /// </summary>
    public static string JoinFlavourNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return string.Empty;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        foreach (var raw in notes.Split(','))
        {
            var trimmed = raw.Trim().Normalize(NormalizationForm.FormC);
            if (trimmed.Length > 0 && seen.Add(trimmed))
                result.Add(trimmed);
        }

        return string.Join(", ", result);
    }

    private static string NormalizeFlavourNotes(string? notes)
    {
        var joined = JoinFlavourNotes(notes);
        if (joined.Length > FlavourNotesMaxLength)
            throw new ArgumentException($"Flavour notes are longer than {FlavourNotesMaxLength} characters.", nameof(notes));

        return joined;
    }
}
