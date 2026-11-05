using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

/// <summary>Deleting the current crop is allowed and simply hides the New Crop section (D1/D4).</summary>
public record DeleteCropCommand(int Id) : IRequest<Result>;
