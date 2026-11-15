using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class MarkOrderRefundedCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser,
    OrderPaymentWorkflow workflow)
    : IRequestHandler<MarkOrderRefundedCommand, Result>
{
    public async Task<Result> Handle(MarkOrderRefundedCommand request, CancellationToken cancellationToken)
    {
        var adminId = await currentUser.GetUserIdAsync(cancellationToken);
        if (adminId is null)
            return Result.Unauthorized();
        var adminName = await currentUser.GetDisplayNameAsync(cancellationToken);

        refresher.DiscardTrackedEntities();

        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.NotFound();

        if (order.PaymentStatus == PaymentStatus.Refunded)
            return Result.Success();

        if (!order.CanMarkRefunded)
            return Result.Error("Only orders with a refund due can be marked as refunded.");

        var result = await workflow.MarkRefundedAsync(
            order, adminId, adminName, request.Reference, request.Note, cancellationToken);

        return result.Status == ResultStatus.Conflict
            ? Result.Conflict("This order was updated by someone else. The page has been refreshed.")
            : result;
    }
}
