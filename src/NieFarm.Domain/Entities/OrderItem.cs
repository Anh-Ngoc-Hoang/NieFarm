using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A line on an <see cref="Order"/>. Owned by the aggregate root — only
/// <see cref="Order"/> creates or mutates these.
/// </summary>
public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }

    /// <summary>Null when the catalog row has since been deleted.</summary>
    public int? ProductId { get; private set; }

    /// <summary>Name and price are copied at checkout so the order never changes retroactively.</summary>
    public string ProductName { get; private set; } = string.Empty;
    public string Variant { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem() { }

    internal static OrderItem Create(
        int? productId, string productName, string variant, string imageUrl,
        decimal unitPrice, int quantity)
    {
        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName.Trim(),
            Variant = variant.Trim(),
            ImageUrl = imageUrl.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity < 1 ? 1 : quantity
        };
    }
}
