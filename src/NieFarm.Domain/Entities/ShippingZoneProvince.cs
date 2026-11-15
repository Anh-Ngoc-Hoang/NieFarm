using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One province (by its code in the in-code catalog) that belongs to a <see cref="ShippingZone"/>.
/// Owned by the zone; only <see cref="ShippingZone.SetProvinces"/> adds or removes these. The code is
/// globally unique, so a province belongs to at most one zone.
/// </summary>
public class ShippingZoneProvince : BaseEntity
{
    public int ShippingZoneId { get; private set; }
    public int ProvinceCode { get; private set; }

    private ShippingZoneProvince() { }

    internal ShippingZoneProvince(int provinceCode) => ProvinceCode = provinceCode;
}
