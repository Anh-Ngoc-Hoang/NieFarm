using Ardalis.Specification;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Reviews;

/// <summary>
/// Decides which (author, product) pairs count as a verified purchase (R-17, "Đã mua hàng").
/// <para>
/// <b>Definition.</b> A pair is verified when at least one <see cref="Order"/> exists with
/// <c>Order.CustomerId == author account id</c>, <c>Order.Status == Completed</c> and an
/// <c>OrderItem.ProductId == product id</c>. Variant, quantity and purchase date do not matter. Guest
/// orders never count (no <c>CustomerId</c>, and matching by phone or email would be forgeable).
/// </para>
/// <para>
/// <b>Why computed at query time, not snapshotted.</b> No migration, no backfill, and a review written
/// while the order was still Delivering gains its label the moment the order is Completed.
/// "Completed" is a terminal status (no cancel, no refund after it), and a bank-transfer order cannot
/// reach it unpaid, so the label is never taken away for an order reason. If a post-delivery
/// return/refund state is ever added, this definition must also look at <c>PaymentStatus</c>/the new
/// status. If review lists grow large, revisit a snapshot column.
/// </para>
/// <para>
/// <b>Data scope (security.md).</b> Call this only for reviews the caller is already allowed to see
/// (the result of <c>ProductReviewsVisibleToSpec</c>, or an admin page behind the Admin role). It
/// returns only the set of (user id, product id) pairs, never an order id, code, date, quantity or
/// variant, and callers must expose nothing more than a <c>bool</c> derived from it.
/// </para>
/// One query regardless of the list size; an empty input runs no query.
/// </summary>
internal static class VerifiedPurchases
{
    public static async Task<IReadOnlySet<(string UserId, int ProductId)>> LoadAsync(
        IReadRepositoryBase<Order> orders,
        IReadOnlyCollection<(string UserId, int ProductId)> pairs,
        CancellationToken cancellationToken)
    {
        if (pairs.Count == 0)
            return new HashSet<(string UserId, int ProductId)>();

        var customerIds = pairs.Select(p => p.UserId).Distinct().ToList();
        var productIds = pairs.Select(p => p.ProductId).Distinct().ToList();

        var rows = await orders.ListAsync(
            new CompletedPurchasesByCustomersSpec(customerIds, productIds), cancellationToken);

        var asked = pairs.ToHashSet();
        var verified = new HashSet<(string UserId, int ProductId)>();
        foreach (var row in rows)
        {
            foreach (var productId in row.ProductIds)
            {
                // The query matches users and products independently, so re-check the exact pair.
                var pair = (row.CustomerId, productId);
                if (asked.Contains(pair))
                    verified.Add(pair);
            }
        }

        return verified;
    }
}
