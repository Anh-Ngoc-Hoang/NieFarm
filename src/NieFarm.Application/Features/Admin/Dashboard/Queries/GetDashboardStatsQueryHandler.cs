using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Dashboard.Dtos;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Dashboard.Queries;

public class GetDashboardStatsQueryHandler(
    IReadRepositoryBase<Product> products,
    IReadRepositoryBase<Article> articles,
    IReadRepositoryBase<Order> orders)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Every read repository resolves its own DbContext, so these are safe in parallel —
        // see .claude/rules/data-access.md.
        var productTask = products.CountAsync(cancellationToken);
        var articleTask = articles.CountAsync(cancellationToken);
        var orderTask = orders.ListAsync(new AllOrdersAdminSpec(), cancellationToken);

        await Task.WhenAll(productTask, articleTask, orderTask);

        var allOrders = orderTask.Result;

        return new DashboardStatsDto(
            productTask.Result,
            articleTask.Result,
            allOrders.Count,
            allOrders.Count(o => !o.IsRead),
            allOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Total),
            allOrders.Take(5).Select(o => new DashboardOrderRow(
                o.Id, o.Code, o.CustomerName, o.Status.ToString(), o.Total, o.CreatedAt)).ToList());
    }
}
