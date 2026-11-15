using Ardalis.Result;
using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

/// <summary>
/// FromStatus is the status the admin was looking at; if the order has since moved, the handler
/// returns Conflict instead of applying a decision made against stale information.
/// FromPaymentStatus does the same for payment, so an admin can never cancel a just-paid order
/// without having seen the refund warning.
/// </summary>
public record UpdateOrderStatusCommand(
    int Id, OrderStatus FromStatus, OrderStatus ToStatus, string? Note,
    PaymentStatus FromPaymentStatus) : IRequest<Result>;
