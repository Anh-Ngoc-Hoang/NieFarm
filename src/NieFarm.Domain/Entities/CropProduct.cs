using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One "shop the crop" link — a catalog product shown alongside a <see cref="Crop"/>. Owned by
/// the aggregate root; only <see cref="Crop.SetProducts"/> creates, reorders or removes these,
/// mirroring ProductSpec/ProductImage on <see cref="Product"/>.
///
/// Links at <see cref="Product.Id"/>, never a variant — the same reasoning as
/// <see cref="CartItem"/>: <see cref="Product.SetOptionsAndVariants"/> clears and re-inserts every
/// option/variant row on every admin save, so only the Product row itself is stable enough to
/// reference.
/// </summary>
public class CropProduct : BaseEntity
{
    public int CropId { get; private set; }
    public int ProductId { get; private set; }
    public int SortOrder { get; private set; }

    private CropProduct() { }

    internal CropProduct(int productId, int sortOrder)
    {
        ProductId = productId;
        SortOrder = sortOrder;
    }

    internal void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
