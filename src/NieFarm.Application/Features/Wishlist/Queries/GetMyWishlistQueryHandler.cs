using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Products;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Wishlist.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Queries;

public class GetMyWishlistQueryHandler(
    IReadRepositoryBase<WishlistItem> wishlist,
    IReadRepositoryBase<Product> products,
    IReadRepositoryBase<Review> reviews,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyWishlistQuery, Result<IReadOnlyList<ProductCardDto>>>
{
    public async Task<Result<IReadOnlyList<ProductCardDto>>> Handle(
        GetMyWishlistQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result<IReadOnlyList<ProductCardDto>>.Unauthorized();

        var rows = await wishlist.ListAsync(new WishlistByUserSpec(userId), cancellationToken);
        if (rows.Count == 0)
            return Result<IReadOnlyList<ProductCardDto>>.Success([]);

        var ids = rows.Select(w => w.ProductId).ToList();
        var visible = await products.ListAsync(new WishlistProductsSpec(ids), cancellationToken);

        // The product query has no order of its own: put the products back in the order they were
        // saved (newest first), dropping rows whose product is hidden or gone.
        var byId = visible.ToDictionary(p => p.Id);
        var ordered = rows
            .Where(w => byId.ContainsKey(w.ProductId))
            .Select(w => byId[w.ProductId])
            .ToList();

        var cards = await ProductCardAssembler.ToCardsAsync(ordered, reviews, cancellationToken);
        return Result<IReadOnlyList<ProductCardDto>>.Success(cards);
    }
}
