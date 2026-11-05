using MediatR;
using NieFarm.Application.Features.Crops.Dtos;

namespace NieFarm.Application.Features.Crops.Queries;

/// <summary>Null when no crop is current — home omits the whole section (D4), no placeholder.</summary>
public record GetCurrentCropQuery : IRequest<CurrentCropDto?>;
