using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One row of a product's specification table — "Xuất xứ" / "Đắk Lắk". Owned by
/// <see cref="Product"/>; the whole list is only ever rebuilt through
/// <see cref="Product.SetSpecs"/>.
/// </summary>
public class ProductSpec : BaseEntity
{
    public int ProductId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ProductSpec() { }

    internal ProductSpec(string label, string value, int sortOrder)
    {
        Label = label.Trim();
        Value = value.Trim();
        SortOrder = sortOrder;
    }
}
