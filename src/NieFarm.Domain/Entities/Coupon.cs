using System.Text.RegularExpressions;
using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// An admin-managed discount code (docs/plans/R-09-10-11-cart-coupons-shipping.md D12). One coupon per
/// order; Percentage and FixedAmount discount the merchandise subtotal, FreeShipping waives the
/// shipping fee. Money is whole đồng (the percentage result is rounded down).
/// <para>
/// There is no separate redemption table: the order row is the redemption (an active use is an order
/// with this coupon whose status is not Cancelled), so cancelling an order releases the use with no extra
/// step. <see cref="TimesRedeemed"/> is an all-time counter whose job is to make every redemption UPDATE
/// this row, so the shadow RowVersion (CouponConfiguration) detects two orders racing for the last use.
/// A coupon that has been used can no longer change its code or type, and cannot be deleted.
/// </para>
/// </summary>
public class Coupon : BaseEntity
{
    public const int CodeMaxLength = 30;
    public const int DescriptionMaxLength = 200;
    public const decimal MaxFixedAmount = 100_000_000m;
    public const decimal MaxMoney = 1_000_000_000m;
    public const int MaxUses = 1_000_000;

    private static readonly Regex CodePattern =
        new(@"^[A-Z0-9][A-Z0-9_-]{3,29}\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Stored trimmed and upper-cased.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Admin-only note, never shown to customers.</summary>
    public string? Description { get; private set; }

    public CouponType Type { get; private set; }

    /// <summary>Percentage: whole 1-100. FixedAmount: whole đồng, at least 1,000. FreeShipping: 0.</summary>
    public decimal Value { get; private set; }

    /// <summary>Caps a Percentage discount; null for no cap (and always null for other types).</summary>
    public decimal? MaxDiscountAmount { get; private set; }

    /// <summary>Compared with the pre-discount subtotal of the payable lines.</summary>
    public decimal MinOrderSubtotal { get; private set; }

    /// <summary>UTC. Null = valid from the start.</summary>
    public DateTime? StartsAt { get; private set; }

    /// <summary>UTC, exclusive. Null = never expires.</summary>
    public DateTime? EndsAt { get; private set; }

    /// <summary>Maximum number of active (non-cancelled) orders that may carry the code; null = unlimited.</summary>
    public int? UsageLimit { get; private set; }

    /// <summary>Maximum active orders per customer (account or phone); null = unlimited.</summary>
    public int? PerCustomerLimit { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>All-time counter, never decremented (a cancelled order frees its use through the order status).</summary>
    public int TimesRedeemed { get; private set; }

    public bool WaivesShipping => Type == CouponType.FreeShipping;

    private Coupon() { }

    public static Coupon Create(
        string code, string? description, CouponType type, decimal value, decimal? maxDiscountAmount,
        decimal minOrderSubtotal, DateTime? startsAt, DateTime? endsAt,
        int? usageLimit, int? perCustomerLimit, bool isActive)
    {
        var coupon = new Coupon();
        coupon.Apply(code, description, type, value, maxDiscountAmount, minOrderSubtotal,
            startsAt, endsAt, usageLimit, perCustomerLimit, isActive);
        return coupon;
    }

    /// <summary>Throws when the code or type changes after the coupon has been redeemed.</summary>
    public void Update(
        string code, string? description, CouponType type, decimal value, decimal? maxDiscountAmount,
        decimal minOrderSubtotal, DateTime? startsAt, DateTime? endsAt,
        int? usageLimit, int? perCustomerLimit, bool isActive)
    {
        if (TimesRedeemed > 0)
        {
            if (!string.Equals(Normalise(code), Code, StringComparison.Ordinal))
                throw new InvalidOperationException("The code of a coupon that has been used cannot change.");
            if (type != Type)
                throw new InvalidOperationException("The type of a coupon that has been used cannot change.");
        }

        Apply(code, description, type, value, maxDiscountAmount, minOrderSubtotal,
            startsAt, endsAt, usageLimit, perCustomerLimit, isActive);
    }

    private void Apply(
        string code, string? description, CouponType type, decimal value, decimal? maxDiscountAmount,
        decimal minOrderSubtotal, DateTime? startsAt, DateTime? endsAt,
        int? usageLimit, int? perCustomerLimit, bool isActive)
    {
        var normalised = Normalise(code);
        if (!IsValidCode(normalised))
            throw new InvalidOperationException("The coupon code must be 4-30 characters: A-Z, 0-9, hyphen or underscore.");

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (trimmedDescription is { Length: > DescriptionMaxLength })
            throw new InvalidOperationException("The coupon description is too long (max 200 characters).");

        if (!IsValidValue(type, value))
            throw new InvalidOperationException("The coupon value is not valid for its type.");

        if (type == CouponType.Percentage)
        {
            if (maxDiscountAmount is { } cap && !IsWholeAmount(cap, min: 1m, max: MaxMoney))
                throw new InvalidOperationException("The maximum discount must be a whole amount above 0.");
        }
        else if (maxDiscountAmount is not null)
        {
            throw new InvalidOperationException("A maximum discount only applies to a percentage coupon.");
        }

        if (!IsWholeAmount(minOrderSubtotal, min: 0m, max: MaxMoney))
            throw new InvalidOperationException("The minimum order must be a whole amount of 0 or more.");

        if (startsAt is { } from && endsAt is { } to && to <= from)
            throw new InvalidOperationException("The end of the validity window must be after its start.");

        if (usageLimit is { } total && (total < 1 || total > MaxUses))
            throw new InvalidOperationException("The usage limit must be at least 1.");
        if (perCustomerLimit is { } each && (each < 1 || each > MaxUses))
            throw new InvalidOperationException("The per-customer limit must be at least 1.");

        Code = normalised;
        Description = trimmedDescription;
        Type = type;
        Value = value;
        MaxDiscountAmount = maxDiscountAmount;
        MinOrderSubtotal = minOrderSubtotal;
        StartsAt = startsAt;
        EndsAt = endsAt;
        UsageLimit = usageLimit;
        PerCustomerLimit = perCustomerLimit;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        SetUpdatedAt();
    }

    /// <summary>
    /// Called in the same SaveChanges as the order that uses the coupon: the UPDATE is checked against
    /// the row version, so two orders racing for the last use cannot both succeed.
    /// </summary>
    public void RecordRedemption()
    {
        TimesRedeemed++;
        SetUpdatedAt();
    }

    /// <summary>
    /// Decides whether the coupon may be used now. <paramref name="activeUses"/> is the number of non-cancelled
    /// orders carrying it; <paramref name="customerUses"/> is the same count for this customer, or null when
    /// the customer cannot be identified (then the per-customer limit is not checked).
    /// </summary>
    public CouponEligibility CheckEligibility(DateTime nowUtc, decimal subtotal, int activeUses, int? customerUses)
    {
        if (!IsActive) return CouponEligibility.Inactive;
        if (StartsAt is { } from && nowUtc < from) return CouponEligibility.NotStarted;
        if (EndsAt is { } to && nowUtc >= to) return CouponEligibility.Expired;
        if (UsageLimit is { } limit && activeUses >= limit) return CouponEligibility.UsedUp;
        if (subtotal < MinOrderSubtotal) return CouponEligibility.BelowMinimum;
        if (PerCustomerLimit is { } each && customerUses is { } used && used >= each)
            return CouponEligibility.CustomerLimitReached;

        return CouponEligibility.Eligible;
    }

    /// <summary>
    /// The discount on the merchandise subtotal: a percentage rounded down to a whole đồng, capped by
    /// <see cref="MaxDiscountAmount"/>, never more than the subtotal. FreeShipping discounts no merchandise.
    /// </summary>
    public decimal MerchandiseDiscount(decimal subtotal) =>
        ComputeMerchandiseDiscount(Type, Value, MaxDiscountAmount, subtotal);

    /// <summary>The one discount formula, shared with the pricing engine's pure calculator.</summary>
    public static decimal ComputeMerchandiseDiscount(
        CouponType type, decimal value, decimal? maxDiscountAmount, decimal subtotal)
    {
        if (subtotal <= 0m)
            return 0m;

        var discount = type switch
        {
            CouponType.Percentage => Math.Floor(subtotal * value / 100m),
            CouponType.FixedAmount => value,
            _ => 0m
        };

        if (type == CouponType.Percentage && maxDiscountAmount is { } cap)
            discount = Math.Min(discount, cap);

        return Math.Min(discount, subtotal);
    }

    // ----- Code format (single source; Application's CouponCodeFormat wraps it) -----

    public static string Normalise(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>True for an already-normalised code of the form [A-Z0-9][A-Z0-9_-]{3,29}.</summary>
    public static bool IsValidCode(string code) => CodePattern.IsMatch(code);

    public static bool IsValidValue(CouponType type, decimal value) => type switch
    {
        CouponType.Percentage => IsWholeAmount(value, min: 1m, max: 100m),
        CouponType.FixedAmount => IsWholeAmount(value, min: 1_000m, max: MaxFixedAmount),
        CouponType.FreeShipping => value == 0m,
        _ => false
    };

    private static bool IsWholeAmount(decimal value, decimal min, decimal max) =>
        value >= min && value <= max && value == decimal.Truncate(value);
}
