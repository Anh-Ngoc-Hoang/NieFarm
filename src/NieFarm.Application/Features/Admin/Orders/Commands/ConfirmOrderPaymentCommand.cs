using Ardalis.Result;
using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

/// <summary>
/// Records the receipt of a bank transfer. ExpectedStatus is the order status the admin was looking
/// at; if it changed meanwhile (for example the customer cancelled, which would turn this into a
/// refund due), the handler returns Conflict. The amount is never supplied: it is the order total.
/// </summary>
public record ConfirmOrderPaymentCommand(
    int Id, OrderStatus ExpectedStatus, string? Reference, string? Note) : IRequest<Result>;
