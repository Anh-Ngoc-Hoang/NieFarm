using MediatR;
using NieFarm.Application.Features.Carts.Dtos;

namespace NieFarm.Application.Features.Carts.Queries;

/// <summary>
/// A bare DTO, not Result&lt;CartDto&gt;, because it has no failure mode — an absent cart or an
/// unresolvable owner both simply yield CartDto.Empty (mirrors GetUnreadOrderCountQuery).
/// The user id is never accepted here; it is resolved server-side from ICurrentUser.
/// </summary>
public record GetCartQuery(string? AnonymousId) : IRequest<CartDto>;
