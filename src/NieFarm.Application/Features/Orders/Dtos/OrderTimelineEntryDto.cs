using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Dtos;

/// <summary>
/// Public view of one status change. Deliberately carries no notes, admin names or user ids —
/// admin notes are internal.
/// </summary>
public record OrderTimelineEntryDto(OrderStatus Status, DateTime AtUtc, OrderStatusActor Actor);
