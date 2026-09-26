namespace NieFarm.Application.Features.Carts;

/// <summary>
/// The two totals inputs that are not derived from the catalog. Shipping stays a flat constant
/// (out of scope for the catalog-backed cart change); the promo-code field on /thanh-toan is
/// still inert, so Discount is always zero.
/// </summary>
public static class CartPricing
{
    public const decimal ShippingFee = 30_000m;
    public const decimal Discount = 0m;   // the promo-code field is still inert
}
