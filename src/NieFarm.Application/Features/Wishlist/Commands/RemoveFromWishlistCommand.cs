using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Wishlist.Commands;

/// <summary>
/// Removes a product from the signed-in customer's wishlist (R-16 D-21). Idempotent: a product that is
/// not saved is a success. The user comes from ICurrentUser; only the caller's own row can match.
/// </summary>
public record RemoveFromWishlistCommand(int ProductId) : IRequest<Result>;
