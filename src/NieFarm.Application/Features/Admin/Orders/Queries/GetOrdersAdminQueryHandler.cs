using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Orders.Dtos;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public class GetOrdersAdminQueryHandler(IReadRepositoryBase<Order> repository)
    : IRequestHandler<GetOrdersAdminQuery, List<OrderAdminListDto>>
{
    public async Task<List<OrderAdminListDto>> Handle(
        GetOrdersAdminQuery request, CancellationToken cancellationToken)
    {
        var orders = await repository.ListAsync(new AllOrdersAdminSpec(), cancellationToken);

        return orders.Select(o => new OrderAdminListDto(
            o.Id,
            o.Code,
            o.CustomerName,
            o.Phone,
            o.Status,
            o.IsRead,
            o.Items.Sum(i => i.Quantity),
            o.Total,
            o.CreatedAt)).ToList();
    }
}
