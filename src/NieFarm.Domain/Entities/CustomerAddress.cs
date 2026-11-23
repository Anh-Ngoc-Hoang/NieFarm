using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One saved delivery address in a signed-in customer's address book (R-16, D-11). Each row is its
/// own aggregate root keyed to an Identity user id (a plain string here, with the FK configured in
/// Infrastructure), like <see cref="Review"/> and <see cref="Cart"/> — it hangs off no navigation.
///
/// Field lengths match CreateOrderCommandValidator so a saved address can always be used to place an
/// order. A user has at most <see cref="MaxPerUser"/> rows and at most one <see cref="IsDefault"/> (the
/// database backstop is the filtered unique index IX_CustomerAddresses_UserId_Default). The flag is
/// only ever changed through <see cref="MarkDefault"/> / <see cref="ClearDefault"/>, never by
/// <see cref="Update"/>; callers move the default with the two-step clear-then-mark sequence (D-12),
/// never both in one SaveChanges.
/// </summary>
public class CustomerAddress : BaseEntity
{
    public const int MaxPerUser = 10;

    public const int UserIdMaxLength = 450;
    public const int RecipientNameMaxLength = 150;
    public const int PhoneMaxLength = 30;
    public const int StreetMaxLength = 200;
    public const int ProvinceNameMaxLength = 120;
    public const int WardNameMaxLength = 120;

    public string UserId { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;

    /// <summary>Province code from the catalog, used only to re-select the dropdown entry; null in manual-entry mode.</summary>
    public int? ProvinceCode { get; private set; }
    public string ProvinceName { get; private set; } = string.Empty;

    /// <summary>Ward code from the lookup API, used only to re-select the dropdown entry; wards are free text otherwise.</summary>
    public int? WardCode { get; private set; }
    public string WardName { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    private CustomerAddress() { }

    public static CustomerAddress Create(
        string userId, string recipientName, string phone, string street,
        int? provinceCode, string provinceName, int? wardCode, string wardName)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("An address belongs to a user.");

        var address = new CustomerAddress { UserId = userId };
        address.Update(recipientName, phone, street, provinceCode, provinceName, wardCode, wardName);
        return address;
    }

    /// <summary>Never touches <see cref="IsDefault"/> — see <see cref="MarkDefault"/> / <see cref="ClearDefault"/>.</summary>
    public void Update(
        string recipientName, string phone, string street,
        int? provinceCode, string provinceName, int? wardCode, string wardName)
    {
        RecipientName = Require(recipientName, RecipientNameMaxLength, "Recipient name");
        Phone = Require(phone, PhoneMaxLength, "Phone");
        Street = Require(street, StreetMaxLength, "Street");
        ProvinceName = Require(provinceName, ProvinceNameMaxLength, "Province");
        WardName = Require(wardName, WardNameMaxLength, "Ward");
        ProvinceCode = provinceCode;
        WardCode = wardCode;
        SetUpdatedAt();
    }

    /// <summary>
    /// Flag changes deliberately leave <see cref="BaseEntity.UpdatedAt"/> alone: it means "last edited
    /// content", which is what picks the successor when the default is deleted.
    /// </summary>
    public void MarkDefault() => IsDefault = true;

    public void ClearDefault() => IsDefault = false;

    private static string Require(string value, int maxLength, string label)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException($"{label} is required.");
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{label} must be at most {maxLength} characters.");
        return trimmed;
    }
}
