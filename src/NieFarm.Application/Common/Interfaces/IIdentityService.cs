using Ardalis.Result;
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
}
