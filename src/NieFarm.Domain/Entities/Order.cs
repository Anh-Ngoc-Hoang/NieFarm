using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Aggregate root for a customer order. Line items are only added through
/// <see cref="AddItem"/>, and every money value is recomputed here rather than
/// accepted from the caller.
/// <para>
/// Status moves one step forward only: New -> Confirmed -> Delivering -> Completed, and
/// New/Confirmed/Delivering -> Cancelled. Completed and Cancelled are terminal. Customers may only
/// cancel while New. The matrix lives here (<see cref="AllowedNextStatuses"/>) and is the single
/// source of truth; Application handlers check it first to return friendly errors.
/// </para>
/// <para>
/// History invariant: every status change after R-07 appends exactly one
/// <see cref="OrderStatusChange"/> row. Orders that predate it may lack intermediate rows, so never
/// assume the last history ToStatus equals <see cref="Status"/>.
/// </para>
/// <para>
/// Hooks: payment guards (R-01) belong in <see cref="CanChangeStatusTo"/> /
/// <see cref="IsCancellableByCustomer"/>; restocking (R-06) belongs in OrderStatusWorkflow.
/// </para>
/// </summary>
public class Order : BaseEntity
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> Transitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.New] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
            [OrderStatus.Confirmed] = [OrderStatus.Delivering, OrderStatus.Cancelled],
            [OrderStatus.Delivering] = [OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.Completed] = [],
            [OrderStatus.Cancelled] = []
        };

    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusChange> _statusChanges = [];

    /// <summary>Human-facing reference used by the public order lookup, e.g. NF123456.</summary>
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

    public IReadOnlyCollection<OrderStatusChange> StatusChanges => _statusChanges.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    public decimal Total => Subtotal + ShippingFee - Discount;

    private Order() { }

    public static Order Create(
        string code, string? customerId, string customerName, string phone, string? email,
        string shippingAddress, string? note, PaymentMethod paymentMethod,
        decimal shippingFee, decimal discount)
    {
        var order = new Order
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

        order._statusChanges.Add(OrderStatusChange.Placed(order.CreatedAt, order.CustomerId));
        return order;
    }

    public void AddItem(
        int? productId, string productName, string variant, string imageUrl,
        decimal unitPrice, int quantity)
    {
        _items.Add(OrderItem.Create(productId, productName, variant, imageUrl, unitPrice, quantity));
        SetUpdatedAt();
    }

    public static IReadOnlyList<OrderStatus> AllowedNextStatuses(OrderStatus from) =>
        Transitions.TryGetValue(from, out var next) ? next : [];

    public bool CanChangeStatusTo(OrderStatus to) => AllowedNextStatuses(Status).Contains(to);

    /// <summary>Customers may cancel only before Nie Farm confirms the order.</summary>
    public bool IsCancellableByCustomer => Status == OrderStatus.New;

    public bool IsFinal => AllowedNextStatuses(Status).Count == 0;

    /// <summary>Admin status change. Cancelling requires a reason.</summary>
    public void ChangeStatus(OrderStatus to, string adminUserId, string? adminName, string? note)
    {
        if (!CanChangeStatusTo(to))
            throw new InvalidOperationException($"An order cannot move from {Status} to {to}.");
        if (to == OrderStatus.Cancelled && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("A reason is required to cancel an order.");

        _statusChanges.Add(OrderStatusChange.Create(
            Status, to, OrderStatusActor.Admin, adminUserId, adminName, note));
        Status = to;
        SetUpdatedAt();
    }

    /// <summary>Customer self-cancel (New only). Re-flags the order as unread so the admin notices.</summary>
    public void CancelByCustomer(string? customerUserId, string? customerName, string? reason)
    {
        if (!IsCancellableByCustomer)
            throw new InvalidOperationException($"A {Status} order cannot be cancelled by the customer.");

        _statusChanges.Add(OrderStatusChange.Create(
            Status, OrderStatus.Cancelled, OrderStatusActor.Customer, customerUserId, customerName, reason));
        Status = OrderStatus.Cancelled;
        IsRead = false;
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
