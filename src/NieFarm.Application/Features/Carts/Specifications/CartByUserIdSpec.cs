using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Specifications;

/// <summary>The signed-in user's cart to merge into (MergeGuestCartCommand).</summary>
public sealed class CartByUserIdSpec : Specification<Cart>, ISingleResultSpecification<Cart>
{
    public CartByUserIdSpec(string userId)
    {
        Query.Where(c => c.UserId == userId).Include(c => c.Items);
    }
}
