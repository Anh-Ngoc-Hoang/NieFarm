using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders;

/// <summary>Maps an order's status history to the public timeline DTOs (no notes, names or ids).</summary>
public static class OrderTimeline
{
    public static IReadOnlyList<OrderTimelineEntryDto> From(Order order) =>
        order.StatusChanges
            .OrderBy(c => c.ChangedAt).ThenBy(c => c.Id)
            .Select(c => new OrderTimelineEntryDto(c.ToStatus, c.ChangedAt, c.ChangedBy))
            .ToList();
}
