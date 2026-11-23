using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Wishlist.Commands;

/// <summary>
/// Saves a visible product to the signed-in customer's wishlist (R-16 D-21). Idempotent: a product that
/// is already saved is a success and adds nothing. The user comes from ICurrentUser.
/// </summary>
public record AddToWishlistCommand(int ProductId) : IRequest<Result>;
