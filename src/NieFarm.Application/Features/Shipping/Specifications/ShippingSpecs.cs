using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Shipping.Specifications;

public sealed class ShippingSettingsSingletonSpec : Specification<ShippingSettings>
{
    public ShippingSettingsSingletonSpec() =>
        Query.Where(s => s.SingletonKey == ShippingSettings.SingletonValue);
}

/// <summary>Zones with their provinces, in display order. activeOnly is what pricing and the public pages use.</summary>
public sealed class ShippingZonesSpec : Specification<ShippingZone>
{
    public ShippingZonesSpec(bool activeOnly)
    {
        if (activeOnly)
            Query.Where(z => z.IsActive);

        Query.Include(z => z.Provinces)
            .OrderBy(z => z.SortOrder).ThenBy(z => z.Name);
    }
}

public sealed class ShippingZoneByIdSpec : Specification<ShippingZone>
{
    public ShippingZoneByIdSpec(int id) =>
        Query.Where(z => z.Id == id).Include(z => z.Provinces);
}
