using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public class GetUnreadOrderCountQueryHandler(IReadRepositoryBase<Order> repository)
    : IRequestHandler<GetUnreadOrderCountQuery, int>
{
    public Task<int> Handle(GetUnreadOrderCountQuery request, CancellationToken cancellationToken) =>
        repository.CountAsync(new UnreadOrdersSpec(), cancellationToken);
}
