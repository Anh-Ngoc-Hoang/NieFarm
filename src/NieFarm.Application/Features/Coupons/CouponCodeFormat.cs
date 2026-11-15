using NieFarm.Application.Features.Orders;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Coupons;

/// <summary>
/// Normalisation of what a shopper typed (or an admin saves) into a coupon code, and of the phone key that
/// identifies a customer for the per-customer limit. The format itself lives on <see cref="Coupon"/>.
/// </summary>
public static class CouponCodeFormat
{
    /// <summary>Phone keys are stored in nvarchar(20); a longer number keeps its last digits.</summary>
    public const int PhoneKeyMaxLength = 20;

    /// <summary>Trims and upper-cases; false when the result is not a well-formed code (non-ASCII, too short, ...).</summary>
    public static bool TryNormalise(string? input, out string code)
    {
        code = Coupon.Normalise(input);
        return Coupon.IsValidCode(code);
    }

    /// <summary>The per-customer identity of a phone number, or null when it has no digits.</summary>
    public static string? PhoneKey(string? phone)
    {
        var normalised = PhoneNumberNormalizer.Normalise(phone);
        if (normalised.Length == 0)
            return null;

        return normalised.Length <= PhoneKeyMaxLength ? normalised : normalised[^PhoneKeyMaxLength..];
    }

    /// <summary>The throttle owner key of a cart: the account when signed in, otherwise the anonymous cookie id.</summary>
    public static string? OwnerKey(string? userId, string? anonymousId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
            return "u:" + userId;
        return string.IsNullOrWhiteSpace(anonymousId) ? null : "a:" + anonymousId;
    }
}
