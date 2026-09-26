using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Join row tying a <see cref="ProductVariant"/> to one <see cref="ProductOptionValue"/> —
/// one row per option, so a variant names exactly one value on every axis.
/// </summary>
public class ProductVariantValue : BaseEntity
{
    public int ProductVariantId { get; private set; }
    public int ProductOptionValueId { get; private set; }
    public ProductOptionValue OptionValue { get; private set; } = null!;

    private ProductVariantValue() { }

    /// <summary>
    /// Sets the navigation rather than the foreign key: when a whole matrix is inserted at once
    /// the option value has no Id yet, and EF fills the FK in after it assigns one.
    /// </summary>
    internal ProductVariantValue(ProductOptionValue optionValue)
    {
        OptionValue = optionValue;
    }
}
