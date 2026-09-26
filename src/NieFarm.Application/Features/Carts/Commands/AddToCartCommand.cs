using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Carts.Dtos;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// Adds the given product/variant selection to the shopper's cart (or increments an existing
/// matching line). No price, name or image ever crosses this boundary — they are always
/// re-derived from the Products table inside the handler (security.md).
/// </summary>
public record AddToCartCommand(
    int ProductId,
    IReadOnlyList<string> SelectedOptionValues,
    int Quantity,
    string? AnonymousId) : IRequest<Result<CartDto>>;
