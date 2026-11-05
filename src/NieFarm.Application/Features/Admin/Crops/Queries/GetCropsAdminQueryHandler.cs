using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Crops.Dtos;
using NieFarm.Application.Features.Crops.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Crops.Queries;

public class GetCropsAdminQueryHandler(IReadRepositoryBase<Crop> repository)
    : IRequestHandler<GetCropsAdminQuery, List<CropAdminListDto>>
{
    public async Task<List<CropAdminListDto>> Handle(
        GetCropsAdminQuery request, CancellationToken cancellationToken)
    {
        var crops = await repository.ListAsync(new AllCropsAdminSpec(), cancellationToken);

        return crops.Select(c => new CropAdminListDto(
            c.Id,
            c.Year,
            c.SeasonLabel,
            c.Headline,
            c.ImageUrl,
            c.IsCurrent,
            c.Products.Count)).ToList();
    }
}
