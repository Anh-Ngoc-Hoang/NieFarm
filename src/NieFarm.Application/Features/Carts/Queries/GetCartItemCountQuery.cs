using MediatR;

namespace NieFarm.Application.Features.Carts.Queries;

/// <summary>Cheaper than the full CartDto — used only by CartState for the header badge.</summary>
public record GetCartItemCountQuery(string? AnonymousId) : IRequest<int>;
