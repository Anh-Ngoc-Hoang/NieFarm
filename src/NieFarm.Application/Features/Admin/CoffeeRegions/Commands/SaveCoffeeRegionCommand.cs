using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Commands;

/// <summary>
/// Updates the region row addressed by <paramref name="Key"/>. There is no Id, no create path and
/// no delete command: the three rows are fixed by the map and created only by the seeder
/// (docs/plans/home-vietnam-coffee-stories.md D7). <paramref name="Areas"/> is the full desired
/// list in display order; CoffeeRegion.SetAreas diffs it against the stored rows.
/// </summary>
public record SaveCoffeeRegionCommand(
    CoffeeRegionKey Key,
    string Title,
    string? Tagline,
    string Description,
    string? Altitude,
    string? Varieties,
    string? Climate,
    string? FlavorProfile,
    string? ImageUrl,
    string? ImageAlt,
    IReadOnlyList<CoffeeRegionAreaInput> Areas) : IRequest<Result>;
