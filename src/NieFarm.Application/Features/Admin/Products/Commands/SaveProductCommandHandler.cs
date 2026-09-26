using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Products.Commands;

public class SaveProductCommandHandler(
    IRepositoryBase<Product> repository,
    IReadRepositoryBase<ProductCategory> categories,
    IImageStorage imageStorage,
    ISlugGenerator slugGenerator,
    IHtmlSanitizer htmlSanitizer)
    : IRequestHandler<SaveProductCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveProductCommand request, CancellationToken cancellationToken)
    {
        // Existence check only — the entity is discarded, so a read repository is safe here.
        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<int>.Error("Select an existing category.");

        var slug = slugGenerator.Generate(
            string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);

        var clash = await repository.FirstOrDefaultAsync(
            new ProductBySlugSpec(slug, request.Id), cancellationToken);
        if (clash is not null)
            return Result<int>.Error($"Slug \"{slug}\" is already used by another product.");

        var options = MapOptions(request.Options);
        var variants = MapVariants(request.Variants);
        var specs = MapSpecs(request.Specs);
        var features = request.Features ?? [];

        // Both come from the rich-text editor, so they are stored markup rather than plain text.
        var summary = htmlSanitizer.Sanitize(request.Summary);
        var description = htmlSanitizer.Sanitize(request.Description);

        if (request.Id is { } id)
        {
            // Loaded with the matrix through the write repository, so the same context tracks
            // the join rows it has to delete and saves everything together.
            var existing = await repository.FirstOrDefaultAsync(
                new ProductByIdWithDetailsSpec(id), cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            var previousImages = CollectImageUrls(existing);

            existing.Update(
                request.Name, slug, request.CategoryId, request.Price, request.OldPrice, request.ImageUrl,
                request.ImageAlt, summary, description, request.Badge,
                request.IsFeatured, request.IsVisible, request.SortOrder);

            existing.SetOptionsAndVariants(options, variants);
            existing.SetSpecs(specs);
            existing.SetFeatures(features);
            existing.SetImages(request.GalleryImageUrls ?? []);

            await repository.UpdateAsync(existing, cancellationToken);

            // Diff rather than compare: the same upload can sit on the product and on several
            // variants, and it must survive as long as anything still points at it.
            var keptImages = CollectImageUrls(existing);
            var removed = previousImages
                .Where(url => !keptImages.Contains(url))
                .Where(ProductImageLimits.IsManagedImage)
                .ToList();

            if (removed.Count > 0)
                await imageStorage.DeleteManyAsync(removed, cancellationToken);

            return Result<int>.Success(existing.Id);
        }

        var product = Product.Create(
            request.Name, slug, request.CategoryId, request.Price, request.OldPrice, request.ImageUrl,
            request.ImageAlt, summary, description, request.Badge,
            request.IsFeatured, request.IsVisible, request.SortOrder);

        product.SetOptionsAndVariants(options, variants);
        product.SetSpecs(specs);
        product.SetFeatures(features);
        product.SetImages(request.GalleryImageUrls ?? []);

        await repository.AddAsync(product, cancellationToken);
        return Result<int>.Success(product.Id);
    }

    /// <summary>Every image the product currently references, main photo plus variant overrides.</summary>
    private static HashSet<string> CollectImageUrls(Product product)
    {
        var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            urls.Add(product.ImageUrl);

        foreach (var variant in product.Variants)
        {
            if (!string.IsNullOrWhiteSpace(variant.ImageUrl))
                urls.Add(variant.ImageUrl);
        }

        foreach (var image in product.Images)
            urls.Add(image.Url);

        return urls;
    }

    private static List<(string Name, IReadOnlyList<string> Values)> MapOptions(
        List<ProductOptionInput>? options) =>
        (options ?? [])
            .Select(o => (o.Name ?? string.Empty, (IReadOnlyList<string>)(o.Values ?? [])))
            .ToList();

    private static List<(string Label, string Value)> MapSpecs(List<ProductSpecInput>? specs) =>
        (specs ?? [])
            .Select(s => (s.Label ?? string.Empty, s.Value ?? string.Empty))
            .ToList();

    private static List<(IReadOnlyList<int> ValueIndexes, decimal Price, string? ImageUrl, bool IsAvailable)>
        MapVariants(List<ProductVariantInput>? variants) =>
        (variants ?? [])
            .Select(v => ((IReadOnlyList<int>)(v.ValueIndexes ?? []), v.Price, v.ImageUrl, v.IsAvailable))
            .ToList();
}
