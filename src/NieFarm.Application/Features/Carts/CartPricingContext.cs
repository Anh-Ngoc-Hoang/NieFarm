namespace NieFarm.Application.Features.Carts;

/// <summary>
/// The pricing inputs that are not part of the cart itself. The province is a pricing input only
/// (the name the shopper chose or typed); it is never stored on the cart. Phase 3 adds the coupon
/// inputs here.
/// </summary>
public record CartPricingContext(string? Province)
{
    public static CartPricingContext None { get; } = new((string?)null);
}
