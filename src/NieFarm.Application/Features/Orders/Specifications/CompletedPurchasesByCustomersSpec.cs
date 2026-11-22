using Ardalis.Specification;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// Completed orders of the given accounts that contain at least one of the given products, projected
/// to (customer id, matching product ids) — one round trip for a whole review list. Guest orders
/// (<c>CustomerId == null</c>) never match. Used only by
/// <see cref="NieFarm.Application.Features.Reviews.VerifiedPurchases"/>.
/// </summary>
public sealed class CompletedPurchasesByCustomersSpec : Specification<Order, CustomerPurchaseRow>
{
    public CompletedPurchasesByCustomersSpec(
        IReadOnlyCollection<string> customerIds,
        IReadOnlyCollection<int> productIds)
    {
        Query.Where(o => o.CustomerId != null
                      && customerIds.Contains(o.CustomerId)
                      && o.Status == OrderStatus.Completed
                      && o.Items.Any(i => i.ProductId != null && productIds.Contains(i.ProductId.Value)))
             .AsNoTracking();

        Query.Select(o => new CustomerPurchaseRow(
            o.CustomerId!,
            o.Items.Where(i => i.ProductId != null && productIds.Contains(i.ProductId.Value))
                   .Select(i => i.ProductId!.Value)
                   .ToList()));
    }
}
