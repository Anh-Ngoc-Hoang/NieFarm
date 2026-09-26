using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One axis a product varies along, e.g. "Đơn vị" or "Mức độ xay". Owned by
/// <see cref="Product"/> — the whole set is only ever rebuilt through
/// <see cref="Product.SetOptionsAndVariants"/>.
/// </summary>
public class ProductOption : BaseEntity
{
    private readonly List<ProductOptionValue> _values = [];

    public int ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    public IReadOnlyCollection<ProductOptionValue> Values => _values.AsReadOnly();

    private ProductOption() { }

    internal ProductOption(string name, int sortOrder)
    {
        Name = name.Trim();
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds a value and returns it, so <see cref="Product"/> can hold the reference and wire it
    /// into the variants it builds in the same pass.
    /// </summary>
    internal ProductOptionValue AddValue(string label, int sortOrder)
    {
        var value = new ProductOptionValue(label, sortOrder);
        _values.Add(value);
        return value;
    }
}
