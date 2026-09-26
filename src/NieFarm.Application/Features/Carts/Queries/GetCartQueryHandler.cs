using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Queries;

public class GetCartQueryHandler(IReadRepositoryBase<Cart> repository, ICurrentUser currentUser, CartAssembler assembler)
    : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return CartDto.Empty;

        var cart = await repository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);

        return await assembler.BuildAsync(cart, cancellationToken);
    }
}
