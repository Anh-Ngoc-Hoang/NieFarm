using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// One page of the admin order list: search, status and payment filters, newest first (R-20).
/// The same class serves <c>CountAsync</c> (Ardalis applies only the criteria there, no Include,
/// ordering or Skip/Take) and <c>ListAsync</c>, so the total can never disagree with the page.
/// <c>Items</c> are included for the page only (at most a page of orders), so the item count and
/// <see cref="Order.Total"/> come from the domain rather than a second copy of the formula in SQL.
/// </summary>
public sealed class OrdersAdminPageSpec : Specification<Order>
{
    /// <param name="search">Already normalised; null means no search. Matches code, name or phone as "contains".</param>
    /// <param name="skip">Rows to skip. Pass 0 together with a null <paramref name="take"/> for an unpaged list.</param>
    public OrdersAdminPageSpec(
        string? search,
        OrderStatus? status,
        OrderPaymentFilter payment,
        int skip = 0,
        int? take = null)
    {
        // string.Contains becomes a parameterised LIKE with %, _ and [ escaped, so they stay literal.
        if (!string.IsNullOrEmpty(search))
            Query.Where(o => o.Code.Contains(search)
                          || o.CustomerName.Contains(search)
                          || o.Phone.Contains(search));

        if (status is { } s)
            Query.Where(o => o.Status == s);

        switch (payment)
        {
            case OrderPaymentFilter.CashOnDelivery:
                Query.Where(o => o.PaymentMethod == PaymentMethod.CashOnDelivery);
                break;
            case OrderPaymentFilter.AwaitingPayment:
                Query.Where(o => o.PaymentStatus == PaymentStatus.AwaitingPayment);
                break;
            case OrderPaymentFilter.Paid:
                Query.Where(o => o.PaymentStatus == PaymentStatus.Paid);
                break;
            case OrderPaymentFilter.RefundDue:
                Query.Where(o => o.PaymentStatus == PaymentStatus.RefundDue);
                break;
            case OrderPaymentFilter.Refunded:
                Query.Where(o => o.PaymentStatus == PaymentStatus.Refunded);
                break;
        }

        Query.Include(o => o.Items).AsNoTracking();

        // Ends in Id: with Skip/Take a non-unique order can repeat or drop an order across pages.
        Query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);

        if (skip > 0)
            Query.Skip(skip);
        if (take is > 0)
            Query.Take(take.Value);
    }
}
