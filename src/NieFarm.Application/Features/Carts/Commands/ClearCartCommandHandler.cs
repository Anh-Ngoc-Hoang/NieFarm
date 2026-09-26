using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Specifications;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts.Commands;

public class ClearCartCommandHandler(IRepositoryBase<DomainCart> repository, ICurrentUser currentUser)
    : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return Result.Error("Không xác định được giỏ hàng.");

        var cart = await repository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);

        if (cart is null)
            return Result.Success();

        // Per D13, a successful order deletes the cart row rather than clearing it — this
        // command deletes the row too, so an empty husk never lingers. Cart.Clear() still
        // exists on the aggregate for the catalog-backed future; it has no caller today.
        await repository.DeleteAsync(cart, cancellationToken);

        return Result.Success();
    }
}
