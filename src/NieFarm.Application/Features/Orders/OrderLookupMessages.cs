namespace NieFarm.Application.Features.Orders;

/// <summary>Customer-facing (Vietnamese) messages shared by the order lookup and cancel handlers.</summary>
public static class OrderLookupMessages
{
    public const string LockedOut =
        "Bạn đã tra cứu sai quá nhiều lần. Vui lòng thử lại sau 15 phút hoặc liên hệ Nie Farm để được hỗ trợ.";

    public const string NotCancellable =
        "Đơn hàng đã được Nie Farm xác nhận nên bạn không thể tự hủy. Vui lòng liên hệ Nie Farm để được hỗ trợ.";

    public const string PaidNotCancellable =
        "Nie Farm đã nhận được thanh toán cho đơn hàng này nên bạn không thể tự hủy. Vui lòng liên hệ Nie Farm để được hỗ trợ.";

    public const string CancelAfterTransferWarning =
        "Nếu bạn đã chuyển khoản cho đơn này, vui lòng liên hệ Nie Farm sau khi hủy để được hoàn tiền.";

    public const string Conflict =
        "Đơn hàng vừa được cập nhật. Thông tin mới nhất đã được hiển thị.";

    public const string AccountOrderRequiresSignIn =
        "Đơn hàng này được đặt bằng tài khoản. Vui lòng đăng nhập để hủy đơn.";
}
