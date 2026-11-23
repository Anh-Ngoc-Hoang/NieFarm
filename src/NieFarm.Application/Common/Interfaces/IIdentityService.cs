using Ardalis.Result;
using NieFarm.Application.Features.Account.Profile.Dtos;
using NieFarm.Application.Features.Admin.Accounts.Dtos;
using NieFarm.Application.Features.Admin.Roles.Dtos;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Application-side surface over ASP.NET Identity. Implemented in Infrastructure so the
/// Application layer never references Identity types directly.
/// </summary>
public interface IIdentityService
{
    Task<List<AccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken);

    Task<AccountDto?> GetAccountAsync(string userId, CancellationToken cancellationToken);

    Task<Result<string>> CreateUserAsync(
        string email, string fullName, string password, string role,
        string? phoneNumber, CancellationToken cancellationToken);

    Task<Result> UpdateUserAsync(
        string userId, string fullName, string? password, string role, bool isActive,
        string? phoneNumber, CancellationToken cancellationToken);

    Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken);

    Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);

    Task<int> CountAdminsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Self-service customer registration. Returns Conflict when the email is already
    /// registered (to any account, admin or customer — RequireUniqueEmail is on).
    /// Messages are deliberately not localised here; the caller owns the wording.
    /// </summary>
    Task<Result<string>> RegisterCustomerAsync(
        string email, string fullName, string password, string? phoneNumber,
        CancellationToken cancellationToken);

    /// <summary>A customer's own profile, or null when the user does not exist (R-16).</summary>
    Task<CustomerProfileDto?> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates name and phone only (a blank phone is stored as null). Deliberately does NOT change the
    /// security stamp (R-16 D-06): the caller keeps its session. NotFound when the user is gone; a failed
    /// save is an Error carrying <see cref="Features.Account.AccountErrorCodes.SaveFailed"/>.
    /// </summary>
    Task<Result> UpdateCustomerProfileAsync(
        string userId, string fullName, string? phoneNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the password after verifying the current one. This changes the security stamp, so it must
    /// run from a Razor Page that then refreshes the cookie (R-16 D-05). A wrong current password counts
    /// toward lockout; the failed Result carries Identity codes (or <see cref="Features.Account.AccountErrorCodes"/>)
    /// in <c>Errors</c>, never the passwords. NotFound when the user is gone.
    /// </summary>
    Task<Result> ChangePasswordAsync(
        string userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}
