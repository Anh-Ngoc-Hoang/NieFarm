using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Carts.Dtos;

namespace NieFarm.Application.Features.Carts.Commands;

public record RemoveCartLineCommand(int ProductId, string VariantKey, string? AnonymousId) : IRequest<Result<CartDto>>;
