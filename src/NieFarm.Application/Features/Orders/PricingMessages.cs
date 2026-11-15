namespace NieFarm.Application.Features.Orders;

/// <summary>Customer-facing (Vietnamese) pricing messages used when an order is placed.</summary>
public static class PricingMessages
{
    public const string TotalChanged =
        "Tổng tiền đơn hàng vừa thay đổi. Vui lòng kiểm tra lại trước khi đặt hàng.";

    public const string ZeroTotalBankTransfer =
        "Đơn hàng 0 ₫ không cần chuyển khoản. Vui lòng chọn Thanh toán khi nhận hàng (COD).";
}
