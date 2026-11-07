using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class UpdateOrderStatusCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser,
    OrderStatusWorkflow workflow)
    : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var adminId = await currentUser.GetUserIdAsync(cancellationToken);
        if (adminId is null)
            return Result.Unauthorized();
        var adminName = await currentUser.GetDisplayNameAsync(cancellationToken);

        // Decide against the database, not against state cached in this circuit's shared context.
        refresher.DiscardTrackedEntities();

        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.NotFound();

        // Already there (double-click, retry, the other party did the same): idempotent no-op.
        if (order.Status == request.ToStatus)
            return Result.Success();

        if (order.Status != request.FromStatus)
            return Result.Conflict(
                $"This order was updated by someone else — it is now {order.Status}. The page has been refreshed.");

        if (!order.CanChangeStatusTo(request.ToStatus))
            return Result.Error($"An order cannot move from {order.Status} to {request.ToStatus}.");

        var result = await workflow.ChangeByAdminAsync(
            order, request.ToStatus, adminId, adminName, request.Note, cancellationToken);

        return result.Status == ResultStatus.Conflict
            ? Result.Conflict("This order was updated by someone else. The page has been refreshed.")
            : result;
    }
}
