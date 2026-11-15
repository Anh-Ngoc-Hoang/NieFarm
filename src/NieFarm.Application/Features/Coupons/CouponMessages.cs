using NieFarm.Application.Features.Shipping;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Coupons;

/// <summary>
/// Customer-facing (Vietnamese) coupon copy. Money formatting matches Money.Vnd (vi-VN), following the
/// OrderLookupMessages precedent. Reasons that would help someone guess codes (unknown, inactive, not
/// started, expired, used up) all share one generic sentence.
/// </summary>
public static class CouponMessages
{
    public const string Invalid = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";

    public const string LockedOut =
        "Bạn đã nhập sai mã giảm giá quá nhiều lần. Vui lòng thử lại sau 15 phút.";

    public const string CustomerLimitReached = "Bạn đã dùng hết lượt cho mã giảm giá này.";

    public const string CustomerLimitReachedAtCheckout =
        CustomerLimitReached + " Vui lòng gỡ mã giảm giá để tiếp tục đặt hàng.";

    public const string FreeShippingAlready =
        "Đơn hàng đã được miễn phí vận chuyển nên mã này không giảm thêm.";

    public const string NoDiscount = "Mã này không giảm thêm cho đơn hàng hiện tại.";

    public const string Busy =
        "Mã giảm giá vừa được nhiều người sử dụng cùng lúc. Vui lòng bấm Xác nhận đặt hàng lại.";

    public const string EmptyCart = "Giỏ hàng chưa có sản phẩm để áp dụng mã giảm giá.";

    public static string BelowMinimum(decimal minOrderSubtotal) =>
        $"Mã này áp dụng cho đơn hàng có tạm tính từ {ShippingMessages.Vnd(minOrderSubtotal)}.";

    public static string Applied(string code, decimal discount) =>
        $"Đã áp dụng mã {code}: giảm {ShippingMessages.Vnd(discount)}";

    public static string NotApplied(string code, string reason) =>
        $"Mã {code} chưa được áp dụng: {reason}";

    /// <summary>The reason a coupon that is not eligible cannot be used, generic unless it helps the honest shopper.</summary>
    public static string Reason(CouponEligibility eligibility, Coupon coupon) => eligibility switch
    {
        CouponEligibility.BelowMinimum => BelowMinimum(coupon.MinOrderSubtotal),
        CouponEligibility.CustomerLimitReached => CustomerLimitReached,
        _ => Invalid
    };

    /// <summary>True for the outcomes that count as a failed guess (everything but minimum and per-customer limit).</summary>
    public static bool CountsAsFailure(CouponEligibility eligibility) =>
        eligibility is CouponEligibility.Inactive or CouponEligibility.NotStarted
            or CouponEligibility.Expired or CouponEligibility.UsedUp;
}
