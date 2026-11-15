using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Commands;

public class CancelLookedUpOrderCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher,
    IOrderLookupThrottle throttle,
    OrderStatusWorkflow workflow)
    : IRequestHandler<CancelLookedUpOrderCommand, Result>
{
    public async Task<Result> Handle(CancelLookedUpOrderCommand request, CancellationToken cancellationToken)
    {
        if (!OrderCode.TryNormalise(request.Code, out var code))
            return Result.NotFound();
        var phone = PhoneNumberNormalizer.Normalise(request.Phone);

        if (throttle.IsLockedOut(code, phone))
            return Result.Error(OrderLookupMessages.LockedOut);

        refresher.DiscardTrackedEntities();

        var order = await repository.FirstOrDefaultAsync(new OrderByCodeSpec(code), cancellationToken);

        // Unknown code and wrong phone share one branch, exactly like the lookup.
        if (order is null || !PhoneNumberNormalizer.Matches(order.Phone, phone))
        {
            throttle.RecordFailure(code, phone);
            return Result.NotFound();
        }

        // Account orders must prove ownership by signing in; code + phone is weaker proof.
        if (order.CustomerId is not null)
            return Result.Error(OrderLookupMessages.AccountOrderRequiresSignIn);

        if (order.Status == OrderStatus.Cancelled)
            return Result.Success();

        if (!order.IsCancellableByCustomer)
            return Result.Error(order.PaymentStatus == PaymentStatus.Paid
                ? OrderLookupMessages.PaidNotCancellable
                : OrderLookupMessages.NotCancellable);

        var result = await workflow.CancelByCustomerAsync(order, null, null, request.Reason, cancellationToken);

        return result.Status == ResultStatus.Conflict
            ? Result.Conflict(OrderLookupMessages.Conflict)
            : result;
    }
}
