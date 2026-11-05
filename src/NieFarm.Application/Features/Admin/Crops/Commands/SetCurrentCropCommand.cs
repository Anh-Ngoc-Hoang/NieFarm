using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

/// <summary>A null <paramref name="Id"/> clears every current crop, hiding the home page section (D1/D4).</summary>
public record SetCurrentCropCommand(int? Id) : IRequest<Result>;
