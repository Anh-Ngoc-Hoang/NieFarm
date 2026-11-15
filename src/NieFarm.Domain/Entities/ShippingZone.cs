using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A named group of provinces with its own shipping fee. A province resolves to a zone through its
/// catalog code (never a name, so admin spelling cannot drift). An inactive zone is ignored when
/// pricing, so its provinces pay the base fee.
/// </summary>
public class ShippingZone : BaseEntity
{
    public const int NameMaxLength = 100;

    private readonly List<ShippingZoneProvince> _provinces = [];

    public string Name { get; private set; } = string.Empty;
    public decimal Fee { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<ShippingZoneProvince> Provinces => _provinces.AsReadOnly();

    private ShippingZone() { }

    public static ShippingZone Create(string name, decimal fee, int sortOrder, bool isActive)
    {
        var zone = new ShippingZone();
        zone.Update(name, fee, sortOrder, isActive);
        return zone;
    }

    public void Update(string name, decimal fee, int sortOrder, bool isActive)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0 || trimmed.Length > NameMaxLength)
            throw new InvalidOperationException("The zone name is required (max 100 characters).");
        if (!ShippingSettings.IsValidFee(fee))
            throw new InvalidOperationException("The zone fee must be a whole amount from 0 to 1,000,000.");

        Name = trimmed;
        Fee = fee;
        SortOrder = sortOrder;
        IsActive = isActive;
        SetUpdatedAt();
    }

    /// <summary>
    /// Diffs against the current provinces: codes already present are kept, new codes are added,
    /// unlisted ones removed. Throws on a duplicate code or an empty list.
    /// </summary>
    public void SetProvinces(IEnumerable<int> provinceCodes)
    {
        var codes = provinceCodes.ToList();
        if (codes.Count == 0)
            throw new InvalidOperationException("A zone needs at least one province.");
        if (codes.Distinct().Count() != codes.Count)
            throw new InvalidOperationException("A province is listed twice.");

        var wanted = codes.ToHashSet();
        _provinces.RemoveAll(p => !wanted.Contains(p.ProvinceCode));

        var existing = _provinces.Select(p => p.ProvinceCode).ToHashSet();
        foreach (var code in codes.Where(c => !existing.Contains(c)))
            _provinces.Add(new ShippingZoneProvince(code));

        SetUpdatedAt();
    }
}
