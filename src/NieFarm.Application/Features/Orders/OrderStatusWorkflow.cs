using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Features.Orders.Notifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders;

/// <summary>
/// The single seam through which every order status write passes. Handlers have already loaded a
/// fresh order and checked the transition; this applies it, saves, and announces it.
/// <para>
/// R-06 (stock): restock on a move to Cancelled right here, before UpdateAsync, so it lands in the
/// same SaveChanges (data-access.md). R-01 (payment): add guards to Order.CanChangeStatusTo /
/// IsCancellableByCustomer. Today cancelling affects nothing else: there is no stock and no payment
/// state.
/// </para>
/// </summary>
public sealed class OrderStatusWorkflow(IRepositoryBase<Order> repository, IPublisher publisher)
{
    public Task<Result> CancelByCustomerAsync(
        Order order, string? userId, string? name, string? reason, CancellationToken ct) =>
        ApplyAsync(order, () => order.CancelByCustomer(userId, name, reason), ct);

    public Task<Result> ChangeByAdminAsync(
        Order order, OrderStatus to, string adminId, string? adminName, string? note, CancellationToken ct) =>
        ApplyAsync(order, () => order.ChangeStatus(to, adminId, adminName, note), ct);

    private async Task<Result> ApplyAsync(Order order, Action change, CancellationToken ct)
    {
        var from = order.Status;
        change();

        // R-06: restock here when order.Status == Cancelled (same save).
        try
        {
            await repository.UpdateAsync(order, ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Conflict();
        }

        var last = order.StatusChanges.Last();
        await publisher.Publish(new OrderStatusChangedNotification(
            order.Id, order.Code, from, order.Status, last.ChangedBy, last.ChangedAt), ct);

        return Result.Success();
    }
}
