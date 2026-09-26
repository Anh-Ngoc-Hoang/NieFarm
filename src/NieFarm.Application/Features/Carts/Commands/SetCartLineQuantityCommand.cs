using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Carts.Dtos;

namespace NieFarm.Application.Features.Carts.Commands;

public record SetCartLineQuantityCommand(int ProductId, string VariantKey, int Quantity, string? AnonymousId)
    : IRequest<Result<CartDto>>;
