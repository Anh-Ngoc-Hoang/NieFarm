namespace NieFarm.Application.Features.Orders;

/// <summary>Payment filter of the admin order list. COD is a payment method, the rest are payment statuses.</summary>
public enum OrderPaymentFilter
{
    All = 0,
    AwaitingPayment,
    Paid,
    RefundDue,
    Refunded,
    CashOnDelivery
}
