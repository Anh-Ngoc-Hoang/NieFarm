using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook;

/// <summary>Vietnamese customer-facing wording for the address book (R-16). Fixed strings; no input is echoed.</summary>
public static class AddressBookMessages
{
    public const string RecipientRequired = "Vui lòng nhập họ và tên người nhận.";
    public const string RecipientTooLong = "Họ và tên quá dài (tối đa 150 ký tự).";
    public const string PhoneRequired = "Vui lòng nhập số điện thoại.";
    public const string PhoneInvalid = "Số điện thoại không hợp lệ.";
    public const string StreetRequired = "Vui lòng nhập địa chỉ cụ thể.";
    public const string StreetTooLong = "Địa chỉ cụ thể quá dài (tối đa 200 ký tự).";
    public const string WardRequired = "Vui lòng chọn phường/xã.";
    public const string WardTooLong = "Tên phường/xã quá dài (tối đa 120 ký tự).";
    public const string ProvinceInvalid = "Vui lòng chọn tỉnh/thành phố hợp lệ.";
    public const string InvalidId = "Địa chỉ không hợp lệ.";

    public static readonly string LimitReached =
        $"Sổ địa chỉ đã đủ {CustomerAddress.MaxPerUser} địa chỉ. Vui lòng xóa bớt một địa chỉ trước khi thêm mới.";

    public const string NotFound = "Không tìm thấy địa chỉ. Có thể địa chỉ đã được xóa, vui lòng tải lại trang.";
}
