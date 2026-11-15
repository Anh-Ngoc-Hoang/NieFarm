using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// Products by id, with their category. Used by GetCurrentCropQueryHandler (visibleOnly: true, so
/// a hidden or deleted product silently drops off the home page) and SaveCropCommandHandler's
/// existence check (visibleOnly: false — an admin can still list a hidden product, see D3).
/// <paramref name="includeVariants"/> (default true) also loads Options/Variants so the result can
/// feed a product card (HasOptions/IsAvailable); pass false for a plain existence check.
/// </summary>
public sealed class ProductsByIdsSpec : Specification<Product>
{
    public ProductsByIdsSpec(IReadOnlyCollection<int> ids, bool visibleOnly, bool includeVariants = true)
    {
        Query.Where(p => ids.Contains(p.Id));

        if (visibleOnly)
            Query.Where(p => p.IsVisible);

        Query.Include(p => p.Category);

        if (includeVariants)
        {
            Query.Include(p => p.Options).ThenInclude(o => o.Values)
                .Include(p => p.Variants).ThenInclude(v => v.Values)
                .AsSplitQuery();
        }
    }
}
