using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// The one cart command that carries an explicit UserId (D14a) rather than resolving it from
/// ICurrentUser: its callers are trusted server-side code, and the login caller runs in a
/// Razor Page request scope where ICurrentUser's AuthenticationStateProvider-first resolution
/// has no state set by any component (see docs/plans/cart-quantity-removal.md §10).
/// </summary>
public record MergeGuestCartCommand(string AnonymousId, string UserId) : IRequest<Result>;
