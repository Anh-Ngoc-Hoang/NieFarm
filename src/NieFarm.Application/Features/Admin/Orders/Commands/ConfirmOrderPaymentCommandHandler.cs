using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class ConfirmOrderPaymentCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser,
    OrderPaymentWorkflow workflow)
    : IRequestHandler<ConfirmOrderPaymentCommand, Result>
{
    public async Task<Result> Handle(ConfirmOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var adminId = await currentUser.GetUserIdAsync(cancellationToken);
        if (adminId is null)
            return Result.Unauthorized();
        var adminName = await currentUser.GetDisplayNameAsync(cancellationToken);

        // Decide against the database, not against state cached in the circuit-wide shared context.
        refresher.DiscardTrackedEntities();

        // Write repository with Items: Order.Total is computed from them (see OrderForPaymentSpec).
        var order = await repository.FirstOrDefaultAsync(new OrderForPaymentSpec(request.Id), cancellationToken);
        if (order is null)
            return Result.NotFound();

        // Idempotent: a double-click, or another admin already recorded it. No history row.
        if (order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.RefundDue or PaymentStatus.Refunded)
            return Result.Success();

        if (order.PaymentStatus == PaymentStatus.NotApplicable)
            return Result.Error("This is a cash-on-delivery order; there is no transfer to record.");

        if (order.Status != request.ExpectedStatus)
            return Result.Conflict(
                $"This order was updated by someone else — it is now {order.Status}. The page has been refreshed.");

        var result = await workflow.ConfirmAsync(
            order, OrderStatusActor.Admin, adminId, adminName,
            request.Reference, request.Note, null, null, null, cancellationToken);

        return result.Status == ResultStatus.Conflict
            ? Result.Conflict("This order was updated by someone else. The page has been refreshed.")
            : result;
    }
}
