using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Specifications;

/// <summary>The guest cart to merge from (MergeGuestCartCommand).</summary>
public sealed class CartByAnonymousIdSpec : Specification<Cart>, ISingleResultSpecification<Cart>
{
    public CartByAnonymousIdSpec(string anonymousId)
    {
        Query.Where(c => c.AnonymousId == anonymousId).Include(c => c.Items);
    }
}
