using MediatR;
using NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Queries;

/// <summary>Every coffee region, ordered by key. Normally exactly three rows.</summary>
public record GetCoffeeRegionsAdminQuery : IRequest<List<CoffeeRegionAdminListDto>>;
