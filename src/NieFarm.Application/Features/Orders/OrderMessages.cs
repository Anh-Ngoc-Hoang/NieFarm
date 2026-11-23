using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders;

/// <summary>Customer-facing (Vietnamese) messages for the account order pages (buy again).</summary>
public static class OrderMessages
{
    public const string ReorderConflict =
        "Giỏ hàng vừa được cập nhật ở nơi khác. Vui lòng thử mua lại một lần nữa.";

    public const string ReorderNothingAdded =
        "Không có sản phẩm nào được thêm vào giỏ hàng.";

    public static string ReorderSkipReasonText(ReorderSkipReason reason) => reason switch
    {
        ReorderSkipReason.NoLongerSold => "Sản phẩm không còn được bán.",
        ReorderSkipReason.OptionsChanged => "Phân loại đã thay đổi. Vui lòng chọn lại trên trang sản phẩm.",
        ReorderSkipReason.Unavailable => "Sản phẩm tạm hết hàng.",
        ReorderSkipReason.CartFull =>
            $"Giỏ hàng đã đầy (tối đa {Cart.MaxDistinctItems} sản phẩm khác nhau).",
        _ => "Không thể thêm sản phẩm này vào giỏ hàng."
    };
}
