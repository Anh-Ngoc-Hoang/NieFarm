namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// The signed-in user, resolved by Infrastructure from the current Blazor circuit's
/// authentication state. Handlers use this rather than accepting a user id as command
/// input, so a customer-scoped query can never be pointed at another customer's data.
/// </summary>
public interface ICurrentUser
{
    /// <summary>The Identity user id, or null for an anonymous visitor.</summary>
    Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// A display name for the current user (the "FullName" claim, falling back to the name
    /// claim or the email local-part), or null when there is none to offer. Used to snapshot
    /// <c>Review.AuthorName</c> at submit time without a database round trip.
    /// </summary>
    Task<string?> GetDisplayNameAsync(CancellationToken cancellationToken = default);
}
