using MediatR;
using NieFarm.Application.Features.Admin.Crops.Dtos;

namespace NieFarm.Application.Features.Admin.Crops.Queries;

public record GetCropsAdminQuery : IRequest<List<CropAdminListDto>>;
