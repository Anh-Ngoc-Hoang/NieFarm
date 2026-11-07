using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Orders.Commands;

/// <summary>
/// A guest cancels a New order proven by code + phone. The handler re-verifies both server-side
/// and shares the lookup's throttle; nothing from the earlier lookup query is trusted.
/// </summary>
public record CancelLookedUpOrderCommand(string Code, string Phone, string? Reason) : IRequest<Result>;
