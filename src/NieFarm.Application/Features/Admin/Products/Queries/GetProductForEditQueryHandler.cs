using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Products.Queries;

public class GetProductForEditQueryHandler(IReadRepositoryBase<Product> repository)
    : IRequestHandler<GetProductForEditQuery, ProductEditDto?>
{
    public async Task<ProductEditDto?> Handle(
        GetProductForEditQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(
            new ProductByIdWithDetailsSpec(request.Id), cancellationToken);
        if (product is null)
            return null;

        var orderedOptions = product.Options.OrderBy(o => o.SortOrder).ToList();

        var options = orderedOptions
            .Select(o => new ProductOptionInput(
                o.Name,
                o.Values.OrderBy(v => v.SortOrder).Select(v => v.Label).ToList()))
            .ToList();

        // The form addresses values positionally, so every stored option value id is mapped
        // back to its (option, value) position once, up front.
        var positionByValueId = new Dictionary<int, (int OptionIndex, int ValueIndex)>();
        for (var optionIndex = 0; optionIndex < orderedOptions.Count; optionIndex++)
        {
            var values = orderedOptions[optionIndex].Values.OrderBy(v => v.SortOrder).ToList();
            for (var valueIndex = 0; valueIndex < values.Count; valueIndex++)
                positionByValueId[values[valueIndex].Id] = (optionIndex, valueIndex);
        }

        var variants = new List<ProductVariantInput>();
        foreach (var variant in product.Variants.OrderBy(v => v.SortOrder))
        {
            var indexes = new int[orderedOptions.Count];
            var resolved = 0;

            foreach (var value in variant.Values)
            {
                if (!positionByValueId.TryGetValue(value.ProductOptionValueId, out var position))
                    continue;

                indexes[position.OptionIndex] = position.ValueIndex;
                resolved++;
            }

            // A variant that does not name every axis cannot be shown in the grid; skipping it
            // lets the admin re-save a clean matrix instead of hitting a broken form.
            if (resolved != orderedOptions.Count)
                continue;

            variants.Add(new ProductVariantInput(
                [.. indexes], variant.Price, variant.ImageUrl, variant.IsAvailable));
        }

        return new ProductEditDto(
            product.Id,
            product.Name,
            product.Slug,
            product.CategoryId,
            product.Price,
            product.OldPrice,
            product.ImageUrl,
            product.ImageAlt,
            product.Summary,
            product.Description,
            product.Badge,
            product.IsFeatured,
            product.IsVisible,
            product.SortOrder,
            options,
            variants,
            product.Specs.OrderBy(s => s.SortOrder)
                .Select(s => new ProductSpecInput(s.Label, s.Value)).ToList(),
            product.Features.OrderBy(f => f.SortOrder)
                .Select(f => f.Text).ToList(),
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList());
    }
}
