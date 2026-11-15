using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

/// <summary>Undoes a recorded payment (Paid back to Awaiting) while nothing has shipped. Reason required.</summary>
public record RevertOrderPaymentCommand(int Id, string Reason) : IRequest<Result>;
