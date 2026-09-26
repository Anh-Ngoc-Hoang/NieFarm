namespace NieFarm.Application.Features.Carts.Dtos;

public record CartLineDto(
    int ProductId,
    string VariantKey,
    string Slug,          // so the checkout line can link back to /san-pham/{slug}
    string Name,
    string Variant,       // display form, "1kg / Xay pha phin"; empty when the product has no options
    string ImageUrl,
    string ImageAlt,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    bool IsAvailable);

public record CartDto(
    IReadOnlyList<CartLineDto> Lines,
    int ItemCount,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total)
{
    public bool IsEmpty => Lines.Count == 0;
    public bool HasUnavailableLines => Lines.Any(l => !l.IsAvailable);
    public static CartDto Empty { get; } = new([], 0, 0m, 0m, 0m, 0m);
}
