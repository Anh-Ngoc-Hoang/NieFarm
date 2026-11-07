using Ardalis.Result;
using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

/// <summary>
/// FromStatus is the status the admin was looking at; if the order has since moved, the handler
/// returns Conflict instead of applying a decision made against stale information.
/// </summary>
public record UpdateOrderStatusCommand(
    int Id, OrderStatus FromStatus, OrderStatus ToStatus, string? Note) : IRequest<Result>;
