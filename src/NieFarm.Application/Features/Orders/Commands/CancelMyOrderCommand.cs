using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Orders.Commands;

/// <summary>A signed-in customer cancels their own New order. The user id comes from ICurrentUser.</summary>
public record CancelMyOrderCommand(int Id, string? Reason) : IRequest<Result>;
