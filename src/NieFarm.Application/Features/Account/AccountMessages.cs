namespace NieFarm.Application.Features.Account;

/// <summary>
/// Vietnamese customer-facing wording for the account pages (R-16). Fixed strings only: nothing
/// here ever echoes user input, so a message is safe to carry in a redirect or render verbatim.
/// </summary>
public static class AccountMessages
{
    public const string ProfileNotFound = "Không tìm thấy thông tin tài khoản. Vui lòng đăng nhập lại.";
    public const string ProfileSaveFailed = "Không thể lưu thông tin. Vui lòng tải lại trang và thử lại.";

    public const string NameRequired = "Vui lòng nhập họ và tên.";
    public const string NameTooLong = "Họ và tên quá dài (tối đa 150 ký tự).";
    public const string PhoneInvalid = "Số điện thoại không hợp lệ.";

    public const string PasswordTooLong = "Mật khẩu quá dài (tối đa 128 ký tự).";
    public const string CurrentPasswordRequired = "Vui lòng nhập mật khẩu hiện tại.";
    public const string CurrentPasswordWrong = "Mật khẩu hiện tại không đúng.";
    public const string NewPasswordRequired = "Vui lòng nhập mật khẩu mới.";
    public const string NewPasswordRejected =
        "Mật khẩu mới chưa đáp ứng yêu cầu: ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.";
    public const string NewPasswordSameAsCurrent = "Mật khẩu mới phải khác mật khẩu hiện tại.";
    public const string ConfirmMismatch = "Mật khẩu xác nhận không khớp.";
    public const string LockedOut =
        "Tài khoản tạm thời bị khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau.";
    public const string PasswordChangeFailed = "Không thể đổi mật khẩu. Vui lòng thử lại.";
}
