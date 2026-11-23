using MediatR;
using NieFarm.Application.Common.Models;
using NieFarm.Application.Features.Admin.Orders.Dtos;
using NieFarm.Application.Features.Orders;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

/// <summary>
/// One page of the admin order list (R-20). <paramref name="Search"/> matches order code, customer
/// name or phone as "contains" and is normalised by the handler. <paramref name="Page"/> and
/// <paramref name="PageSize"/> are only requests: the handler clamps them and the returned
/// <see cref="PagedResult{T}"/> carries the values actually used.
/// </summary>
public record GetOrdersAdminQuery(
    string? Search = null,
    OrderStatus? Status = null,
    OrderPaymentFilter Payment = OrderPaymentFilter.All,
    int Page = 1,
    int PageSize = GetOrdersAdminQuery.DefaultPageSize) : IRequest<PagedResult<OrderAdminListDto>>
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;
}
