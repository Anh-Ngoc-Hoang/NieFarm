using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

/// <summary>
/// Creates when <paramref name="Id"/> is null, otherwise updates that row. Never touches
/// IsCurrent — a new crop is never current; use SetCurrentCropCommand to change which crop is
/// shown on the home page (D1).
/// </summary>
public record SaveCropCommand(
    int? Id,
    int Year,
    string SeasonLabel,
    string Headline,
    string Story,
    string? Region,
    string? Altitude,
    string? Varieties,
    string? Process,
    string? TastingNotes,
    string ImageUrl,
    string ImageAlt,
    IReadOnlyList<int> ProductIds) : IRequest<Result<int>>;
