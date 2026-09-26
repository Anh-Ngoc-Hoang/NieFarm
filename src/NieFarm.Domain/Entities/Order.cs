using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Aggregate root for a customer order. Line items are only added through
/// <see cref="AddItem"/>, and every money value is recomputed here rather than
/// accepted from the caller.
/// </summary>
public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];

    /// <summary>Human-facing reference used by the public order lookup, e.g. NF12345.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// The Identity user id of the account that placed this order, or null for a guest
    /// checkout. Kept as a plain string so the Domain never references Identity types;
    /// the foreign key to AspNetUsers is declared in OrderConfiguration.
    /// </summary>
    public string? CustomerId { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public string? Note { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.New;

    public PaymentMethod PaymentMethod { get; private set; } = PaymentMethod.CashOnDelivery;

    /// <summary>Set when an admin first opens the order, so the sidebar badge can count unread ones.</summary>
    public bool IsRead { get; private set; }

    public decimal ShippingFee { get; private set; }
    public decimal Discount { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    public decimal Total => Subtotal + ShippingFee - Discount;

    private Order() { }

    public static Order Create(
        string code, string? customerId, string customerName, string phone, string? email,
        string shippingAddress, string? note, PaymentMethod paymentMethod,
        decimal shippingFee, decimal discount)
    {
        return new Order
        {
            Code = code.Trim().ToUpperInvariant(),
            CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId,
            CustomerName = customerName.Trim(),
            Phone = phone.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            ShippingAddress = shippingAddress.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            PaymentMethod = paymentMethod,
            ShippingFee = shippingFee,
            Discount = discount
        };
    }

    public void AddItem(
        int? productId, string productName, string variant, string imageUrl,
        decimal unitPrice, int quantity)
    {
        _items.Add(OrderItem.Create(productId, productName, variant, imageUrl, unitPrice, quantity));
        SetUpdatedAt();
    }

    public void ChangeStatus(OrderStatus status)
    {
        Status = status;
        SetUpdatedAt();
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        SetUpdatedAt();
    }
}
