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

    public static string Vi(PaymentMethod p) => p switch
    {
        PaymentMethod.CashOnDelivery => "Thanh toán khi nhận hàng (COD)",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        _ => p.ToString()
    };
}
