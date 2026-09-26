using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Carts.Commands;

public record ClearCartCommand(string? AnonymousId) : IRequest<Result>;
