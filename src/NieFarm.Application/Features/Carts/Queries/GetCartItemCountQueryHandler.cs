using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Queries;

public class GetCartItemCountQueryHandler(IReadRepositoryBase<Cart> repository, ICurrentUser currentUser)
    : IRequestHandler<GetCartItemCountQuery, int>
{
    public async Task<int> Handle(GetCartItemCountQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return 0;

        var cart = await repository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);

        return cart?.TotalQuantity ?? 0;
    }
}
