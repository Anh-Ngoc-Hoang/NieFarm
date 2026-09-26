using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// Idempotent — removing an absent line, or operating on a cart row that does not exist, is a
/// success returning the rebuilt (possibly empty) cart. Deliberate divergence from
/// OfficeCoffee's Result.NotFound(), which is safe there only because its ids are opaque —
/// here a double-click or a stale UI must stay harmless.
/// </summary>
public class RemoveCartLineCommandHandler(IRepositoryBase<DomainCart> repository, CartAssembler assembler, ICurrentUser currentUser)
    : IRequestHandler<RemoveCartLineCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(RemoveCartLineCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return Result<CartDto>.Error("Không xác định được giỏ hàng.");

        var cart = await repository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);

        if (cart is null)
            return Result<CartDto>.Success(CartDto.Empty);

        cart.RemoveItem(request.ProductId, request.VariantKey);
        await repository.UpdateAsync(cart, cancellationToken);

        return Result<CartDto>.Success(await assembler.BuildAsync(cart, cancellationToken));
    }
}
