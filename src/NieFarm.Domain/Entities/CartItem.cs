using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A line on a <see cref="Cart"/>. Owned by the aggregate root — only <see cref="Cart"/>
/// creates or mutates these, mirroring OrderItem.
///
/// Line identity is <c>(ProductId, VariantKey)</c>, not <c>ProductVariantId</c>.
/// <see cref="Product.SetOptionsAndVariants"/> clears and re-inserts every
/// <see cref="ProductVariant"/> / <see cref="ProductOptionValue"/> row on every admin save, so a
/// variant FK would either be cascade-deleted (silently emptying live carts on every catalog
/// edit) or block the save. <see cref="VariantKey"/> is the selected option value labels, in the
/// product's own option SortOrder, joined with '|' (empty for a product with no options) — the
/// same positional identity <c>ProductDetailDto.FindVariant</c> already uses, and it survives a
/// re-save unchanged as long as the labels themselves are not renamed.
/// </summary>
public class CartItem : BaseEntity
{
    public int CartId { get; private set; }
    public int ProductId { get; private set; }

    /// <summary>Selected option labels in option SortOrder, joined with '|'. Empty for a product with no options.</summary>
    public string VariantKey { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    private CartItem() { }

    internal CartItem(int productId, string variantKey, int quantity)
    {
        ProductId = productId;
        VariantKey = variantKey;
        Quantity = Math.Clamp(quantity, 1, Cart.MaxQuantityPerItem);
    }

    internal void SetQuantity(int quantity)
    {
        Quantity = Math.Clamp(quantity, 1, Cart.MaxQuantityPerItem);
        SetUpdatedAt();
    }

    internal void Increment(int by)
    {
        Quantity = Math.Clamp(Quantity + by, 1, Cart.MaxQuantityPerItem);
        SetUpdatedAt();
    }
}
