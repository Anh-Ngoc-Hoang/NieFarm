namespace NieFarm.Application.Features.Account;

/// <summary>
/// Codes <c>IIdentityService</c> returns inside a failed <c>Result</c>, so the Application handler
/// (not Infrastructure) owns the Vietnamese wording. Identity's own codes (for example
/// <c>PasswordTooShort</c>) pass through unchanged; these are the ones this app adds or relies on.
/// </summary>
public static class AccountErrorCodes
{
    /// <summary>Identity's own code for a wrong current password.</summary>
    public const string PasswordMismatch = "PasswordMismatch";

    /// <summary>The account is locked out, so the password was not even checked.</summary>
    public const string LockedOut = "LockedOut";

    /// <summary>The user could not be saved (store failure, concurrency conflict).</summary>
    public const string SaveFailed = "SaveFailed";
}
