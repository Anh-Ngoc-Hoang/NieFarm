using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Carts.Dtos;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// Deletes abandoned guest carts (R-23). Sent by the background cleanup service, not by a user:
/// it carries no user or cart identity. <paramref name="IdleCartDays"/> null or 0 switches rule C off.
/// </summary>
public record PurgeAbandonedCartsCommand(
    int EmptyCartIdleDays,
    int? IdleCartDays,
    int BatchSize,
    int MaxBatches) : IRequest<Result<CartPurgeResult>>;
