namespace NieFarm.Domain.Enums;

/// <summary>Payment state of an order. Stored as an int - never renumber.</summary>
public enum PaymentStatus
{
    /// <summary>Cash on delivery: collected by the courier, not tracked by the site.</summary>
    NotApplicable = 0,

    /// <summary>Bank transfer, no money recorded yet.</summary>
    AwaitingPayment = 1,

    Paid = 2,

    /// <summary>Money received but the order was cancelled: Nie Farm owes a refund (done by hand).</summary>
    RefundDue = 3,

    Refunded = 4
}
