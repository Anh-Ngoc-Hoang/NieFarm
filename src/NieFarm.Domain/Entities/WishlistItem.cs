using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One product a signed-in customer saved to their wishlist (R-16, D-19). Each row is its own aggregate
/// root keyed to an Identity user id (a plain string, FK configured in Infrastructure) and a
/// <see cref="Product"/> id, like <see cref="CustomerAddress"/> and <see cref="CartItem"/>: it hangs off no
/// navigation, so the wishlist is queried standalone.
///
/// The link is to the <c>Product</c> row, never a variant, for the same reason as <see cref="CartItem"/>:
/// <c>Product.SetOptionsAndVariants</c> rewrites every variant row on each admin save. A user has at most
/// <see cref="MaxPerUser"/> rows and at most one per product (the database backstop is the unique index on
/// (UserId, ProductId)). A hidden product keeps its row but is not shown; a deleted product takes its rows
/// with it (cascade).
/// </summary>
public class WishlistItem : BaseEntity
{
    public const int MaxPerUser = 200;

    public const int UserIdMaxLength = 450;

    public string UserId { get; private set; } = string.Empty;
    public int ProductId { get; private set; }

    private WishlistItem() { }

    public static WishlistItem Create(string userId, int productId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("A wishlist item belongs to a user.");
        if (productId <= 0)
            throw new InvalidOperationException("A wishlist item needs a product.");

        return new WishlistItem { UserId = userId, ProductId = productId };
    }
}
