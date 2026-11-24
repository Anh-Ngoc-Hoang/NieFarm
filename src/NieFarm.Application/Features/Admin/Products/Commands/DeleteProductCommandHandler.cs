using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Products.Commands;

public class DeleteProductCommandHandler(
    IRepositoryBase<Product> repository,
    IReadRepositoryBase<RoastBatch> roastBatches,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // A batch has a QR printed on real packs (R-26 D7), so its product cannot be deleted from under it.
        // The foreign key is Restrict as well; this check just gives the admin a readable message.
        if (await roastBatches.AnyAsync(new RoastBatchesByProductSpec(request.Id), cancellationToken))
        {
            return Result.Error(
                "This product has roast batches with printed QR labels. Hide the product instead of deleting it.");
        }

        // The matrix has to be tracked for the join rows to be removed with the product.
        var product = await repository.FirstOrDefaultAsync(
            new ProductByIdWithDetailsSpec(request.Id), cancellationToken);
        if (product is null)
            return Result.NotFound();

        var imageUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            imageUrls.Add(product.ImageUrl);

        foreach (var variant in product.Variants)
        {
            if (!string.IsNullOrWhiteSpace(variant.ImageUrl))
                imageUrls.Add(variant.ImageUrl);
        }

        foreach (var image in product.Images)
            imageUrls.Add(image.Url);

        await repository.DeleteAsync(product, cancellationToken);

        var removable = imageUrls.Where(ProductImageLimits.IsManagedImage).ToList();
        if (removable.Count > 0)
            await imageStorage.DeleteManyAsync(removable, cancellationToken);

        return Result.Success();
    }
}
