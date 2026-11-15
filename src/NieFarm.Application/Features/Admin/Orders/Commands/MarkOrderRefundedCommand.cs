using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

/// <summary>Marks a refund (done by hand in the bank app) as completed: RefundDue to Refunded.</summary>
public record MarkOrderRefundedCommand(int Id, string? Reference, string? Note) : IRequest<Result>;
