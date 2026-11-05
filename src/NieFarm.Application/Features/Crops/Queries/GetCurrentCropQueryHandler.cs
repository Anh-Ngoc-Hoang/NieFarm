using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Crops.Dtos;
using NieFarm.Application.Features.Crops.Specifications;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Crops.Queries;

public class GetCurrentCropQueryHandler(
    IReadRepositoryBase<Crop> cropRepository,
    IReadRepositoryBase<Product> productRepository)
    : IRequestHandler<GetCurrentCropQuery, CurrentCropDto?>
{
    public async Task<CurrentCropDto?> Handle(GetCurrentCropQuery request, CancellationToken cancellationToken)
    {
        var crop = await cropRepository.FirstOrDefaultAsync(new CurrentCropSpec(), cancellationToken);
        if (crop is null)
            return null;

        var productIds = crop.Products.Select(p => p.ProductId).ToList();

        // Prices/names/images come from the Products table, never from the browser.
        var products = productIds.Count == 0
            ? []
            : await productRepository.ListAsync(new ProductsByIdsSpec(productIds, visibleOnly: true), cancellationToken);

        var productById = products.ToDictionary(p => p.Id);

        // A link whose product was deleted or hidden is silently dropped (D3) — the admin form
        // still shows it with a "Hidden" badge so the admin understands why it disappeared here.
        var orderedProducts = crop.Products
            .OrderBy(cp => cp.SortOrder)
            .Select(cp => productById.GetValueOrDefault(cp.ProductId))
            .Where(p => p is not null)
            .Select(p => new ProductCardDto(
                p!.Slug,
                p.Name,
                p.Category?.Name ?? string.Empty,
                p.Price,
                p.ImageUrl,
                p.ImageAlt,
                string.IsNullOrWhiteSpace(p.Badge) ? null : p.Badge,
                p.Summary))
            .ToList();

        return new CurrentCropDto(
            crop.Year,
            crop.SeasonLabel,
            crop.Headline,
            CropText.SplitParagraphs(crop.Story),
            crop.Region,
            crop.Altitude,
            crop.Varieties,
            crop.Process,
            CropText.SplitTastingNotes(crop.TastingNotes),
            crop.ImageUrl,
            crop.ImageAlt,
            orderedProducts);
    }
}
