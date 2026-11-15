using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// The one row of shipping settings: the base fee (applies to every province not in an active
/// <see cref="ShippingZone"/>, and to unrecognised addresses) and an optional nationwide free-shipping
/// threshold. <see cref="SingletonKey"/> is always 1; the unique index and a check constraint
/// (ShippingSettingsConfiguration) make a second row impossible, and the seeder inserts a missing row
/// (docs/plans/R-09-10-11-cart-coupons-shipping.md D6). Money is whole đồng.
/// </summary>
public class ShippingSettings : BaseEntity
{
    public const decimal MaxFee = 1_000_000m;
    public const int SingletonValue = 1;

    public int SingletonKey { get; private set; } = SingletonValue;
    public decimal BaseFee { get; private set; }

    /// <summary>Null = shipping is never free. Compared with the subtotal after merchandise discount.</summary>
    public decimal? FreeShippingThreshold { get; private set; }

    private ShippingSettings() { }

    public static ShippingSettings Create(decimal baseFee, decimal? freeShippingThreshold)
    {
        var settings = new ShippingSettings();
        settings.Update(baseFee, freeShippingThreshold);
        return settings;
    }

    public void Update(decimal baseFee, decimal? freeShippingThreshold)
    {
        if (!IsValidFee(baseFee))
            throw new InvalidOperationException("The base fee must be a whole amount from 0 to 1,000,000.");
        if (freeShippingThreshold is { } t && (t <= 0 || t != decimal.Truncate(t)))
            throw new InvalidOperationException("The free-shipping threshold must be a whole amount above 0.");

        BaseFee = baseFee;
        FreeShippingThreshold = freeShippingThreshold;
        SetUpdatedAt();
    }

    public static bool IsValidFee(decimal fee) =>
        fee >= 0 && fee <= MaxFee && fee == decimal.Truncate(fee);
}
