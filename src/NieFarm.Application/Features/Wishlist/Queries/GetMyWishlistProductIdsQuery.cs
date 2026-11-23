using MediatR;

namespace NieFarm.Application.Features.Wishlist.Queries;

/// <summary>
/// The product ids the signed-in customer has saved, newest first, for the heart buttons (R-16 D-21).
/// The user comes from ICurrentUser; an anonymous visitor gets an empty list (never an error), so the
/// shared state can load for any circuit.
/// </summary>
public record GetMyWishlistProductIdsQuery : IRequest<IReadOnlyList<int>>;
