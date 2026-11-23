using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Specifications;

/// <summary>
/// Guest carts that the cleanup (R-23) may delete. Never matches an account cart: the
/// <c>UserId == null</c> condition is part of the one predicate, and the purger re-applies this whole
/// specification inside the DELETE statement.
///
/// Rule A - unreachable: created before <c>unreachableBeforeUtc</c>. The nf_cart cookie has a fixed
/// expiry from issue (see <see cref="CartRetentionPolicy"/>), so nobody can open such a cart.
/// Rule B - an empty cart not modified since <c>emptyIdleBeforeUtc</c>.
/// Rule C - any cart not modified since <c>idleBeforeUtc</c> (null = off; it deletes carts a shopper
/// could still reopen, so it is off by default).
///
/// No Include and no OrderBy on purpose: the spec is also used to build a set-based DELETE.
/// </summary>
public sealed class AbandonedGuestCartsSpec : Specification<Cart>
{
    public AbandonedGuestCartsSpec(DateTime unreachableBeforeUtc, DateTime emptyIdleBeforeUtc, DateTime? idleBeforeUtc)
    {
        var idle = idleBeforeUtc ?? DateTime.MinValue;

        Query.Where(c =>
            c.UserId == null &&
            (c.CreatedAt < unreachableBeforeUtc ||
             (c.UpdatedAt < emptyIdleBeforeUtc && !c.Items.Any()) ||
             c.UpdatedAt < idle));
    }
}
