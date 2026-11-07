using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Notifications;

/// <summary>
/// Published by OrderStatusWorkflow after a status change is saved. No handlers yet (R-03 adds
/// the email ones); MediatR Publish with zero handlers is a no-op.
/// </summary>
public record OrderStatusChangedNotification(
    int OrderId, string Code, OrderStatus? From, OrderStatus To,
    OrderStatusActor Actor, DateTime ChangedAtUtc) : INotification;
