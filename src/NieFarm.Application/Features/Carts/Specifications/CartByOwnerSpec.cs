using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Specifications;

/// <summary>
/// Resolves the single cart owned by a logged-in user (by <see cref="Cart.UserId"/>) or, when
/// there is no user id, by an anonymous browser id (never matching a cart already claimed by a
/// user). The c.UserId == null clause is what stops a stale cookie from ever reaching a cart
/// that has since been claimed by an account.
/// </summary>
public sealed class CartByOwnerSpec : Specification<Cart>, ISingleResultSpecification<Cart>
{
    public CartByOwnerSpec(string? userId, string? anonymousId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            Query.Where(c => c.UserId == userId);
        }
        else
        {
            Query.Where(c => c.AnonymousId == anonymousId && c.UserId == null);
        }

        Query.Include(c => c.Items);
    }
}
