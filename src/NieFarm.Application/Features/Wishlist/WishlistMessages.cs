using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist;

/// <summary>Vietnamese customer-facing wording for the wishlist (R-16). Fixed strings; no input is echoed.</summary>
public static class WishlistMessages
{
    public const string InvalidProduct = "Sản phẩm không hợp lệ.";
    public const string ProductNotFound = "Không tìm thấy sản phẩm. Có thể sản phẩm đã ngừng bán.";

    public static readonly string LimitReached =
        $"Danh sách yêu thích đã đủ {WishlistItem.MaxPerUser} sản phẩm. Vui lòng bỏ bớt một sản phẩm trước khi thêm mới.";
}
