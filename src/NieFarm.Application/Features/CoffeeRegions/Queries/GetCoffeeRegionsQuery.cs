using MediatR;
using NieFarm.Application.Features.CoffeeRegions.Dtos;

namespace NieFarm.Application.Features.CoffeeRegions.Queries;

/// <summary>
/// A bare list, not Result&lt;List&lt;CoffeeRegionDto&gt;&gt;, because it has no failure mode
/// (mirrors GetTestimonialsQuery). Public content — no scoping, no ICurrentUser. Ordered by key.
/// </summary>
public record GetCoffeeRegionsQuery : IRequest<List<CoffeeRegionDto>>;
