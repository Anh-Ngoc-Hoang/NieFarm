using MediatR;
using NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Queries;

/// <summary>Null when the row is missing (seeding failed, or it was deleted by hand in SQL).</summary>
public record GetCoffeeRegionForEditQuery(CoffeeRegionKey Key) : IRequest<CoffeeRegionEditDto?>;
