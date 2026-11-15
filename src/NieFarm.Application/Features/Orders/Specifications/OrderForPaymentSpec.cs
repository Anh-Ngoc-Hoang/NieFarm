using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// Write-load for recording a payment: Items only, no history. Items are needed because Order.Total is
/// computed from them and ConfirmPayment records that amount (it throws without items). Because the
/// aggregate is loaded and tracked by the write context, OrderPaymentWorkflow saves with
/// SaveChangesAsync, not UpdateAsync, which would mark every item Modified and re-save it.
/// </summary>
public sealed class OrderForPaymentSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public OrderForPaymentSpec(int id)
    {
        Query.Include(o => o.Items)
             .Where(o => o.Id == id);
    }
}
