using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Carts.Specifications;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// Faithful port of OfficeCoffee's MergeGuestCartCommandHandler.cs. Idempotent and cheap — a
/// single indexed read returns immediately when there is no guest cart, which is what makes it
/// safe to fire once per circuit (CartState.InitializeCoreAsync) as well as at login
/// (Login.cshtml.cs). No orphan anonymous row survives either path: the fast path re-labels
/// the guest row in place (no copy, no id churn); the slow path sums quantities into the
/// user's cart, then deletes the now-empty guest row.
/// </summary>
public class MergeGuestCartCommandHandler(IRepositoryBase<DomainCart> repository)
    : IRequestHandler<MergeGuestCartCommand, Result>
{
    public async Task<Result> Handle(MergeGuestCartCommand request, CancellationToken cancellationToken)
    {
        var guestCart = await repository.FirstOrDefaultAsync(
            new CartByAnonymousIdSpec(request.AnonymousId), cancellationToken);

        if (guestCart is null || guestCart.Items.Count == 0)
            return Result.Success();

        var userCart = await repository.FirstOrDefaultAsync(
            new CartByUserIdSpec(request.UserId), cancellationToken);

        if (userCart is null)
        {
            // Fast path: re-label the row, no copy, no id churn.
            guestCart.AssignToUser(request.UserId);
            await repository.UpdateAsync(guestCart, cancellationToken);
        }
        else
        {
            userCart.MergeFrom(guestCart);
            await repository.UpdateAsync(userCart, cancellationToken);
            await repository.DeleteAsync(guestCart, cancellationToken);
        }

        return Result.Success();
    }
}
