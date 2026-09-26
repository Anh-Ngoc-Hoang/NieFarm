using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One selectable value of a <see cref="ProductOption"/>, e.g. "500g" or "Xay mịn".
/// Created only through <see cref="ProductOption.AddValue"/>.
/// </summary>
public class ProductOptionValue : BaseEntity
{
    public int ProductOptionId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ProductOptionValue() { }

    internal ProductOptionValue(string label, int sortOrder)
    {
        Label = label.Trim();
        SortOrder = sortOrder;
    }
}
