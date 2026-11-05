using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Crops.Dtos;
using NieFarm.Application.Features.Crops.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Crops.Queries;

public class GetCropForEditQueryHandler(IReadRepositoryBase<Crop> repository)
    : IRequestHandler<GetCropForEditQuery, CropEditDto?>
{
    public async Task<CropEditDto?> Handle(GetCropForEditQuery request, CancellationToken cancellationToken)
    {
        var crop = await repository.FirstOrDefaultAsync(new CropByIdWithProductsSpec(request.Id), cancellationToken);
        if (crop is null)
            return null;

        return new CropEditDto(
            crop.Id,
            crop.Year,
            crop.SeasonLabel,
            crop.Headline,
            crop.Story,
            crop.Region,
            crop.Altitude,
            crop.Varieties,
            crop.Process,
            crop.TastingNotes,
            crop.ImageUrl,
            crop.ImageAlt,
            crop.Products.OrderBy(p => p.SortOrder).Select(p => p.ProductId).ToList(),
            crop.IsCurrent);
    }
}
