using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Additional gallery image for the product detail page. The card/cart-facing primary image
/// remains <see cref="Product.ImageUrl"/> — this is an additive gallery only. Alt text is
/// inherited from <see cref="Product.ImageAlt"/> (or <see cref="Product.Name"/> when blank);
/// there is no per-image alt column.
/// </summary>
public class ProductImage : BaseEntity
{
    public int ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ProductImage() { }

    internal ProductImage(string url, int sortOrder)
    {
        Url = url.Trim();
        SortOrder = sortOrder;
    }
}
