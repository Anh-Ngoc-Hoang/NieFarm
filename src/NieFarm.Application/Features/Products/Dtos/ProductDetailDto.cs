namespace NieFarm.Application.Features.Products.Dtos;

/// <summary>Everything /san-pham/{slug} needs to render one product, keyed off its slug.</summary>
public record ProductDetailDto(
    int Id,
    string Slug,
    string Name,
    string CategoryName,
    decimal Price,                          // base price; a selected variant's price wins
    decimal? OldPrice,
    string? Badge,
    string SummaryHtml,                     // sanitised on write (SaveProductCommandHandler)
    string SummaryPlainText,                // for <meta name="description">
    string DescriptionHtml,                 // sanitised on write
    IReadOnlyList<ProductImageDto> Gallery,
    IReadOnlyList<ProductOptionDto> Options,
    IReadOnlyList<ProductVariantDto> Variants,
    IReadOnlyList<ProductSpecDto> Specs,
    IReadOnlyList<string> Features)
{
    /// <summary>The variant whose value labels exactly match the caller's current selection, if any.</summary>
    public ProductVariantDto? FindVariant(IReadOnlyList<string> selectedLabels) =>
        Variants.FirstOrDefault(v => v.ValueLabels.SequenceEqual(selectedLabels));

    /// <summary>The price for the current selection — the matching variant's price, or the base price.</summary>
    public decimal PriceFor(IReadOnlyList<string> selectedLabels) =>
        FindVariant(selectedLabels)?.Price ?? Price;
}
