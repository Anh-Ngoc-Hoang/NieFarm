using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One bullet in a product's highlights list. Owned by <see cref="Product"/>; the whole list is
/// only ever rebuilt through <see cref="Product.SetFeatures"/>.
/// </summary>
public class ProductFeature : BaseEntity
{
    public int ProductId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ProductFeature() { }

    internal ProductFeature(string text, int sortOrder)
    {
        Text = text.Trim();
        SortOrder = sortOrder;
    }
}
