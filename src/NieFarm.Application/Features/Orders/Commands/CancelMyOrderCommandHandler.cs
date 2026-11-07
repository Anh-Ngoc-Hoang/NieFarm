using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Commands;

public class CancelMyOrderCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser,
    OrderStatusWorkflow workflow)
    : IRequestHandler<CancelMyOrderCommand, Result>
{
    public async Task<Result> Handle(CancelMyOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();
        var displayName = await currentUser.GetDisplayNameAsync(cancellationToken);

        refresher.DiscardTrackedEntities();

        // Ownership is part of the WHERE; "not yours" and "missing" are the same NotFound.
        var order = await repository.FirstOrDefaultAsync(
            new MyOrderForCancelSpec(request.Id, userId), cancellationToken);
        if (order is null)
            return Result.NotFound();

        if (order.Status == OrderStatus.Cancelled)
            return Result.Success();

        if (!order.IsCancellableByCustomer)
            return Result.Error(OrderLookupMessages.NotCancellable);

        var result = await workflow.CancelByCustomerAsync(
            order, userId, displayName, request.Reason, cancellationToken);

        return result.Status == ResultStatus.Conflict
            ? Result.Conflict(OrderLookupMessages.Conflict)
            : result;
    }
}
