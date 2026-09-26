using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Aggregate root for a shopper's cart, owned either by a signed-in Identity user
/// (<see cref="UserId"/>) or by an anonymous browser id carried in the nf_cart cookie
/// (<see cref="AnonymousId"/>) — exactly one of the two is ever set. <see cref="UserId"/> is a
/// plain string so Domain never references Identity types; the foreign key to AspNetUsers is
/// declared in CartConfiguration, exactly as Order.CustomerId does.
///
/// VariantKey comparison is ordinal. Callers must always pass the canonical VariantKey built by
/// CartLineKey from the product's own option labels — never raw, unvalidated browser input — so
/// the aggregate's ordinal comparison stays in agreement with the case-insensitive SQL unique
/// index on (CartId, ProductId, VariantKey).
/// </summary>
public class Cart : BaseEntity
{
    public const int MaxQuantityPerItem = 99;
    public const int MaxDistinctItems = 50;

    private readonly List<CartItem> _items = [];

    /// <summary>Identity user id; null for a guest cart.</summary>
    public string? UserId { get; private set; }

    /// <summary>nf_cart cookie value; null once the cart has been claimed by a user.</summary>
    public string? AnonymousId { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public int TotalQuantity => _items.Sum(i => i.Quantity);

    private Cart() { }

    public static Cart ForUser(string userId) => new() { UserId = userId };

    public static Cart ForAnonymous(string anonymousId) => new() { AnonymousId = anonymousId };

    /// <summary>
    /// Adds a new line for the given (productId, variantKey), or increments the quantity of an
    /// existing matching line. Quantities are clamped to <see cref="MaxQuantityPerItem"/>.
    /// </summary>
    public CartItem AddOrIncrement(int productId, string variantKey, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId && i.VariantKey == variantKey);
        if (existing is not null)
        {
            existing.Increment(quantity);
            SetUpdatedAt();
            return existing;
        }

        if (_items.Count >= MaxDistinctItems)
            throw new InvalidOperationException($"A cart cannot contain more than {MaxDistinctItems} distinct items.");

        var item = new CartItem(productId, variantKey, quantity);
        _items.Add(item);
        SetUpdatedAt();
        return item;
    }

    /// <summary>
    /// True when the given (productId, variantKey) would need a new line but the cart is already
    /// at <see cref="MaxDistinctItems"/>. An existing matching line can always still be
    /// incremented. Lets callers report a friendly message instead of catching the exception
    /// from <see cref="AddOrIncrement"/>.
    /// </summary>
    public bool IsFullFor(int productId, string variantKey)
        => _items.Count >= MaxDistinctItems
           && !_items.Any(i => i.ProductId == productId && i.VariantKey == variantKey);

    /// <summary>Sets the quantity of an existing line. A quantity of 0 or less removes the line.</summary>
    public void SetQuantity(int productId, string variantKey, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.VariantKey == variantKey);
        if (item is null)
            return;

        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.SetQuantity(quantity);
        }

        SetUpdatedAt();
    }

    /// <summary>Removing an absent (productId, variantKey) is a no-op, not an error.</summary>
    public void RemoveItem(int productId, string variantKey)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.VariantKey == variantKey);
        if (item is null)
            return;

        _items.Remove(item);
        SetUpdatedAt();
    }

    /// <summary>
    /// No caller today — kept for a future "clear cart" affordance that may want to empty a row
    /// rather than delete it. Do not remove.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        SetUpdatedAt();
    }

    /// <summary>Re-labels this cart as belonging to the given user (fast merge path — no row copy).</summary>
    public void AssignToUser(string userId)
    {
        UserId = userId;
        AnonymousId = null;
        SetUpdatedAt();
    }

    /// <summary>
    /// Merges each line of <paramref name="other"/> into this cart, summing quantities for
    /// matching (ProductId, VariantKey) lines (clamped) and silently skipping lines beyond
    /// <see cref="MaxDistinctItems"/> so a merge can never fail a sign-in.
    /// </summary>
    public void MergeFrom(Cart other)
    {
        foreach (var item in other.Items)
        {
            if (IsFullFor(item.ProductId, item.VariantKey))
                continue;

            AddOrIncrement(item.ProductId, item.VariantKey, item.Quantity);
        }
    }
}
