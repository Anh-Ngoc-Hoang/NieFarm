namespace NieFarm.Domain.Enums;

/// <summary>How a coupon discounts an order. Stored as an int - never renumber.</summary>
public enum CouponType
{
    /// <summary>Value is a whole percentage (1-100) of the merchandise subtotal, optionally capped.</summary>
    Percentage = 1,

    /// <summary>Value is a whole amount in đồng taken off the merchandise subtotal.</summary>
    FixedAmount = 2,

    /// <summary>Waives the shipping fee; Value is 0.</summary>
    FreeShipping = 3
}
