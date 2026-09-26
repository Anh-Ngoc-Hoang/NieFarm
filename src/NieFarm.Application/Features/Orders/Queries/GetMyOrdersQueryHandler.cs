using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Queries;

public class GetMyOrdersQueryHandler(
    IReadRepositoryBase<Order> repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyOrdersQuery, Result<List<MyOrderListDto>>>
{
    public async Task<Result<List<MyOrderListDto>>> Handle(
        GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var customerId = await currentUser.GetUserIdAsync(cancellationToken);
        if (customerId is null)
            return Result<List<MyOrderListDto>>.Unauthorized();

        var orders = await repository.ListAsync(new MyOrdersSpec(customerId), cancellationToken);
        return Result<List<MyOrderListDto>>.Success(orders.Select(o => new MyOrderListDto(
            o.Id, o.Code, o.Status, o.Items.Sum(i => i.Quantity), o.Total, o.CreatedAt)).ToList());
    }
}
