using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.ProductCategories.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.ProductCategories.Queries;

public class GetProductCategoriesQueryHandler(
    IReadRepositoryBase<ProductCategory> categories,
    IReadRepositoryBase<Product> products)
    : IRequestHandler<GetProductCategoriesQuery, List<ProductCategoryDto>>
{
    public async Task<List<ProductCategoryDto>> Handle(
        GetProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Both reads go through IReadRepositoryBase, so each gets its own DbContext and
        // they are safe to run in parallel — see .claude/rules/data-access.md.
        var categoryTask = categories.ListAsync(cancellationToken);
        var productTask = products.ListAsync(cancellationToken);
        await Task.WhenAll(categoryTask, productTask);

        var counts = productTask.Result
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        return categoryTask.Result
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .Select(c => new ProductCategoryDto(
                c.Id, c.Name, c.Slug, c.IsVisible, c.SortOrder,
                counts.TryGetValue(c.Id, out var count) ? count : 0))
            .ToList();
    }
}
