using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One sellable combination of option values — e.g. "500g / Xay mịn" — with its own price,
/// optional image and availability flag. Owned by <see cref="Product"/>.
/// </summary>
public class ProductVariant : BaseEntity
{
    private readonly List<ProductVariantValue> _values = [];

    public int ProductId { get; private set; }

    /// <summary>Unit price in Vietnamese đồng for this combination.</summary>
    public decimal Price { get; private set; }

    /// <summary>Optional override for the product's main photo.</summary>
    public string? ImageUrl { get; private set; }

    public bool IsAvailable { get; private set; }
    public int SortOrder { get; private set; }

    public IReadOnlyCollection<ProductVariantValue> Values => _values.AsReadOnly();

    private ProductVariant() { }

    internal ProductVariant(decimal price, string? imageUrl, bool isAvailable, int sortOrder)
    {
        Price = price;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        IsAvailable = isAvailable;
        SortOrder = sortOrder;
    }

    internal void AddValue(ProductOptionValue optionValue)
    {
        _values.Add(new ProductVariantValue(optionValue));
    }
}
