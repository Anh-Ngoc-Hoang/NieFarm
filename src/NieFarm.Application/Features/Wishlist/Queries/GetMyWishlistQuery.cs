using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Wishlist.Queries;

/// <summary>
/// The signed-in customer's wishlist as product cards (with ratings), most recently saved first.
/// Hidden products are omitted but their rows are kept (R-16 D-23). User id from ICurrentUser.
/// </summary>
public record GetMyWishlistQuery : IRequest<Result<IReadOnlyList<ProductCardDto>>>;
