using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Models;
using NieFarm.Application.Features.Admin.Orders.Dtos;
using NieFarm.Application.Features.Orders;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Application.Features.Products;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public class GetOrdersAdminQueryHandler(IReadRepositoryBase<Order> repository)
    : IRequestHandler<GetOrdersAdminQuery, PagedResult<OrderAdminListDto>>
{
    public async Task<PagedResult<OrderAdminListDto>> Handle(
        GetOrdersAdminQuery request, CancellationToken cancellationToken)
    {
        // Clamp here, not in a validator: a query returning a plain DTO with a validator would make
        // ValidationBehavior throw, and a bad URL parameter must never be a 500.
        var search = ProductSearchTerm.Normalize(request.Search);
        var status = request.Status is { } s && Enum.IsDefined(s) ? s : (OrderStatus?)null;
        var payment = Enum.IsDefined(request.Payment) ? request.Payment : OrderPaymentFilter.All;
        var pageSize = Math.Clamp(request.PageSize, 1, GetOrdersAdminQuery.MaxPageSize);

        var total = await repository.CountAsync(
            new OrdersAdminPageSpec(search, status, payment), cancellationToken);

        if (total == 0)
            return new PagedResult<OrderAdminListDto>([], 1, pageSize, 0);

        // Past the end falls back to the last page. Page comes from a URL, so clamp before the
        // multiplication below.
        var lastPage = (total + pageSize - 1) / pageSize;
        var page = Math.Clamp(request.Page, 1, lastPage);

        var orders = await repository.ListAsync(
            new OrdersAdminPageSpec(search, status, payment, (page - 1) * pageSize, pageSize),
            cancellationToken);

        var items = orders.Select(o => new OrderAdminListDto(
            o.Id,
            o.Code,
            o.CustomerName,
            o.Phone,
            o.Status,
            o.IsRead,
            o.Items.Sum(i => i.Quantity),
            o.Total,
            o.CreatedAt,
            o.PaymentMethod,
            o.PaymentStatus)).ToList();

        return new PagedResult<OrderAdminListDto>(items, page, pageSize, total);
    }
}
