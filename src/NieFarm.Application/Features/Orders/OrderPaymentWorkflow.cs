using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Features.Orders.Notifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders;

/// <summary>
/// The seam for every payment write (manual confirm, revert, refund, gateway confirm). Handlers have
/// already loaded a fresh order through the write repository and checked intent and state; this
/// applies the domain change, saves, and announces it.
/// <para>
/// It saves with SaveChangesAsync, NOT UpdateAsync (unlike OrderStatusWorkflow): the aggregate is
/// already tracked by the write context in the same handler, so change detection writes exactly the
/// Orders row (with the rowversion check) and the new history row. UpdateAsync would mark every loaded
/// item Modified and re-save it. This is only correct because handlers load through the write
/// repository immediately before calling in here.
/// </para>
/// </summary>
public sealed class OrderPaymentWorkflow(IRepositoryBase<Order> repository, IPublisher publisher)
{
    public Task<Result> ConfirmAsync(
        Order order, OrderStatusActor actor, string? userId, string? name, string? reference, string? note,
        decimal? receivedAmount, string? provider, string? externalId, CancellationToken ct) =>
        ApplyAsync(order, () => order.ConfirmPayment(
            actor, userId, name, reference, note, receivedAmount, provider, externalId), ct);

    public Task<Result> RevertAsync(
        Order order, string adminId, string? adminName, string reason, CancellationToken ct) =>
        ApplyAsync(order, () => order.RevertPayment(adminId, adminName, reason), ct);

    public Task<Result> MarkRefundedAsync(
        Order order, string adminId, string? adminName, string? reference, string? note, CancellationToken ct) =>
        ApplyAsync(order, () => order.MarkRefunded(adminId, adminName, reference, note), ct);

    private async Task<Result> ApplyAsync(Order order, Action change, CancellationToken ct)
    {
        var from = order.PaymentStatus;
        change();

        try
        {
            await repository.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Conflict();
        }

        var last = order.PaymentChanges.Last();
        await publisher.Publish(new OrderPaymentChangedNotification(
            order.Id, order.Code, from, order.PaymentStatus, last.ChangedBy, last.ChangedAt), ct);

        return Result.Success();
    }
}
