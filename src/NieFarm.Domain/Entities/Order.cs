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
/// Payment rules (R-01, bank transfer only; COD is NotApplicable and untracked): Delivering and
/// Completed are blocked unless PaymentStatus is NotApplicable or Paid; a customer may self-cancel
/// only a New order that is NotApplicable or AwaitingPayment; cancelling a Paid order flips it to
/// RefundDue in the same call. Invariant: a payment amount always comes from <see cref="Total"/>
/// (never a caller value, except a gateway's reported amount which must be at least Total), so
/// <see cref="ConfirmPayment"/> requires the items to be loaded. Restocking (R-06) belongs in
/// OrderStatusWorkflow.
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
    private readonly List<OrderPaymentChange> _paymentChanges = [];

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

    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.NotApplicable;

    /// <summary>UTC, when the receipt of the payment was recorded.</summary>
    public DateTime? PaidAt { get; private set; }

    /// <summary>The money actually received (Total for a manual confirmation, the transferred amount for a gateway).</summary>
    public decimal? PaidAmount { get; private set; }

    /// <summary>Set when an admin first opens the order, so the sidebar badge can count unread ones.</summary>
    public bool IsRead { get; private set; }

    public decimal ShippingFee { get; private set; }
    public decimal Discount { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<OrderStatusChange> StatusChanges => _statusChanges.AsReadOnly();

    public IReadOnlyCollection<OrderPaymentChange> PaymentChanges => _paymentChanges.AsReadOnly();

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
            PaymentStatus = paymentMethod switch
            {
                PaymentMethod.BankTransfer => PaymentStatus.AwaitingPayment,
                PaymentMethod.CashOnDelivery => PaymentStatus.NotApplicable,
                _ => throw new ArgumentOutOfRangeException(nameof(paymentMethod), paymentMethod, "Unknown payment method.")
            },
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

    public bool CanChangeStatusTo(OrderStatus to) =>
        AllowedNextStatuses(Status).Contains(to) && !IsBlockedByPayment(to);

    /// <summary>True when shipping (Delivering/Completed) is locked because the transfer is not recorded.</summary>
    public bool IsBlockedByPayment(OrderStatus to) =>
        to is OrderStatus.Delivering or OrderStatus.Completed
        && PaymentStatus is not (PaymentStatus.NotApplicable or PaymentStatus.Paid);

    /// <summary>The matrix moves from the current status that the payment rules also allow.</summary>
    public IReadOnlyList<OrderStatus> AvailableNextStatuses() =>
        AllowedNextStatuses(Status).Where(CanChangeStatusTo).ToList();

    /// <summary>
    /// Customers may cancel only before Nie Farm confirms the order, and not once the money has been
    /// received (they must contact Nie Farm).
    /// </summary>
    public bool IsCancellableByCustomer =>
        Status == OrderStatus.New
        && (PaymentStatus == PaymentStatus.NotApplicable || PaymentStatus == PaymentStatus.AwaitingPayment);

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

        // Money already received: the cancel creates a refund obligation, in the same save.
        if (to == OrderStatus.Cancelled && PaymentStatus == PaymentStatus.Paid)
        {
            _paymentChanges.Add(OrderPaymentChange.Create(
                PaymentStatus.Paid, PaymentStatus.RefundDue, OrderStatusActor.Admin,
                adminUserId, adminName, PaidAmount, null, null));
            PaymentStatus = PaymentStatus.RefundDue;
        }

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

    // ----- Payment (R-01) -----

    /// <summary>Allowed in any order status: late money after a cancel, legacy reconciliation.</summary>
    public bool CanConfirmPayment => PaymentStatus == PaymentStatus.AwaitingPayment;

    /// <summary>
    /// Records the receipt of the transfer. Needs the items loaded (the amount comes from
    /// <see cref="Total"/>). A manual confirmation passes no <paramref name="receivedAmount"/> and records
    /// Total; a gateway passes the amount the bank reported, which must not be below Total.
    /// A cancelled order goes straight to RefundDue.
    /// </summary>
    public void ConfirmPayment(
        OrderStatusActor actor, string? userId, string? name, string? reference, string? note,
        decimal? receivedAmount = null, string? provider = null, string? externalId = null)
    {
        if (!CanConfirmPayment)
            throw new InvalidOperationException($"A payment cannot be recorded while the payment is {PaymentStatus}.");
        if (actor == OrderStatusActor.Customer)
            throw new InvalidOperationException("A customer cannot confirm a payment.");
        if (_items.Count == 0)
            throw new InvalidOperationException("The order items must be loaded to record a payment.");

        var total = Total;
        if (receivedAmount is { } received && received < total)
            throw new InvalidOperationException("The received amount is below the order total.");

        var to = Status == OrderStatus.Cancelled ? PaymentStatus.RefundDue : PaymentStatus.Paid;
        var amount = receivedAmount ?? total;

        _paymentChanges.Add(OrderPaymentChange.Create(
            PaymentStatus, to, actor, userId, name, amount, reference, note, provider, externalId));
        PaymentStatus = to;
        PaidAt = DateTime.UtcNow;
        PaidAmount = amount;

        if (actor == OrderStatusActor.System)
            IsRead = false;

        SetUpdatedAt();
    }

    /// <summary>Nothing has shipped yet, so a mis-attributed transfer can still be undone.</summary>
    public bool CanRevertPayment =>
        PaymentStatus == PaymentStatus.Paid && (Status == OrderStatus.New || Status == OrderStatus.Confirmed);

    public void RevertPayment(string adminUserId, string? adminName, string reason)
    {
        if (!CanRevertPayment)
            throw new InvalidOperationException("The payment can no longer be undone.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("A reason is required to undo a payment.");

        _paymentChanges.Add(OrderPaymentChange.Create(
            PaymentStatus.Paid, PaymentStatus.AwaitingPayment, OrderStatusActor.Admin,
            adminUserId, adminName, PaidAmount, null, reason));
        PaymentStatus = PaymentStatus.AwaitingPayment;
        PaidAt = null;
        PaidAmount = null;
        SetUpdatedAt();
    }

    public bool CanMarkRefunded => PaymentStatus == PaymentStatus.RefundDue;

    public void MarkRefunded(string adminUserId, string? adminName, string? reference, string? note)
    {
        if (!CanMarkRefunded)
            throw new InvalidOperationException("Only an order with a refund due can be marked as refunded.");

        _paymentChanges.Add(OrderPaymentChange.Create(
            PaymentStatus.RefundDue, PaymentStatus.Refunded, OrderStatusActor.Admin,
            adminUserId, adminName, PaidAmount, reference, note));
        PaymentStatus = PaymentStatus.Refunded;
        SetUpdatedAt();
    }
}
