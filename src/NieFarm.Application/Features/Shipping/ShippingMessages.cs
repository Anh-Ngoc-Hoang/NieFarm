using System.Globalization;
using NieFarm.Application.Features.Shipping.Dtos;

namespace NieFarm.Application.Features.Shipping;

/// <summary>Vietnamese customer-facing shipping copy; money formatting matches Money.Vnd (vi-VN).</summary>
public static class ShippingMessages
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public const string ProvinceNotRecognised =
        "Không nhận diện được tỉnh/thành, áp dụng phí vận chuyển mặc định.";

    public const string FreeShippingReached = "Đơn hàng của bạn được miễn phí vận chuyển.";

    public static string Vnd(decimal amount) => amount.ToString("#,##0", Vi) + " ₫";

    /// <summary>The short product-page sentence: "Phí giao hàng 30.000 ₫ ..." with a range when zones differ.</summary>
    public static string FeeSummary(ShippingPolicyDto policy)
    {
        var fee = policy.MinFee == policy.MaxFee
            ? $"Phí giao hàng {Vnd(policy.MaxFee)}"
            : $"Phí giao hàng từ {Vnd(policy.MinFee)} đến {Vnd(policy.MaxFee)} tùy khu vực";

        return policy.FreeShippingThreshold is { } threshold
            ? $"{fee}; miễn phí cho đơn từ {Vnd(threshold)}; giao toàn quốc."
            : $"{fee}; giao toàn quốc.";
    }

    public static string FreeShippingHint(decimal remaining) =>
        $"Mua thêm {Vnd(remaining)} để được miễn phí vận chuyển.";
}
