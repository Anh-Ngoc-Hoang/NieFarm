using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class RevertOrderPaymentCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser,
    OrderPaymentWorkflow workflow)
    : IRequestHandler<RevertOrderPaymentCommand, Result>
{
    public async Task<Result> Handle(RevertOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var adminId = await currentUser.GetUserIdAsync(cancellationToken);
        if (adminId is null)
            return Result.Unauthorized();
        var adminName = await currentUser.GetDisplayNameAsync(cancellationToken);

        refresher.DiscardTrackedEntities();

        // No items needed: the amount being reverted is the stored PaidAmount.
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.NotFound();

        if (order.PaymentStatus == PaymentStatus.AwaitingPayment)
            return Result.Success();

        if (!order.CanRevertPayment)
            return Result.Error(
                $"The payment can no longer be undone because the order is {order.Status} / payment is {order.PaymentStatus}.");

        var result = await workflow.RevertAsync(order, adminId, adminName, request.Reason, cancellationToken);

        return result.Status == ResultStatus.Conflict
            ? Result.Conflict("This order was updated by someone else. The page has been refreshed.")
            : result;
    }
}
