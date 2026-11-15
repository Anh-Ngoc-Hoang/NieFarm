using NieFarm.Domain.Enums;

namespace NieFarm.Web.Content;

/// <summary>
/// Vietnamese display text for order-related enums, for the public storefront only.
/// The admin site keeps rendering the raw enum names in English — see ui.md.
/// </summary>
public static class OrderStatusText
{
    public static string Vi(OrderStatus s) => s switch
    {
        OrderStatus.New => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Delivering => "Đang giao hàng",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => s.ToString()
    };

    /// <summary>Modifier class for <c>.badge-status</c> (nf-pages.css).</summary>
    public static string PillClass(OrderStatus s) => s switch
    {
        OrderStatus.Cancelled => "badge-status--cancelled",
        OrderStatus.Completed => "badge-status--ok",
        _ => "badge-status--pending"
    };

    public static string Vi(PaymentStatus s) => s switch
    {
        PaymentStatus.NotApplicable => "Thanh toán khi nhận hàng",
        PaymentStatus.AwaitingPayment => "Chờ thanh toán",
        PaymentStatus.Paid => "Đã thanh toán",
        PaymentStatus.RefundDue => "Đang hoàn tiền",
        PaymentStatus.Refunded => "Đã hoàn tiền",
        _ => s.ToString()
    };

    /// <summary>Modifier class for <c>.badge-status</c> (nf-pages.css).</summary>
    public static string PaymentPillClass(PaymentStatus s) => s switch
    {
        PaymentStatus.Paid or PaymentStatus.Refunded => "badge-status--ok",
        PaymentStatus.RefundDue => "badge-status--cancelled",
        _ => "badge-status--pending"
    };

    public static string Vi(PaymentMethod p) => p switch
    {
        PaymentMethod.CashOnDelivery => "Thanh toán khi nhận hàng (COD)",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        _ => p.ToString()
    };
}
