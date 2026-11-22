using Ardalis.Specification;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// The visible catalog for <c>/cua-hang</c>: category filter, search, sort and one page of results.
/// The same class serves <c>CountAsync</c> (Ardalis applies only the criteria there — no Include,
/// ordering or Skip/Take) and <c>ListAsync</c>, so the total can never disagree with the page.
/// A product in a hidden category still shows (intended — it appears under "Tất cả").
/// </summary>
public sealed class VisibleProductsSpec : Specification<Product>
{
    /// <param name="categoryIds">Empty or null means "no filter", not "no results".</param>
    /// <param name="search">Already normalised by <see cref="ProductSearchTerm.Normalize"/>; null means no search.</param>
    /// <param name="skip">Rows to skip. Pass 0 together with a null <paramref name="take"/> for an unpaged list.</param>
    public VisibleProductsSpec(
        IReadOnlyCollection<int>? categoryIds,
        string? search,
        ShopSortOrder sort,
        int skip = 0,
        int? take = null)
    {
        Query.Where(p => p.IsVisible);

        if (categoryIds is { Count: > 0 })
            Query.Where(p => categoryIds.Contains(p.CategoryId));

        // string.Contains becomes a parameterised LIKE/CHARINDEX; %, _ and [ stay literal. The
        // Name columns carry Latin1_General_100_CI_AI, so this is case- and accent-insensitive.
        if (!string.IsNullOrEmpty(search))
            Query.Where(p => p.Name.Contains(search) || p.Category!.Name.Contains(search));

        // Options/Variants feed the card's HasOptions/IsAvailable (R-09).
        Query.Include(p => p.Category)
            .Include(p => p.Options).ThenInclude(o => o.Values)
            .Include(p => p.Variants).ThenInclude(v => v.Values)
            .AsSplitQuery()
            // Read-only listing. Identity resolution keeps shared option values as one instance;
            // the repository's context lives as long as the circuit, so tracking would pin every
            // listed entity in memory for nothing.
            .AsNoTrackingWithIdentityResolution();

        // Exactly one ordering chain — two unconditional OrderBy calls on the same builder
        // would silently discard the first. Every chain ends in Id: with Skip/Take and a split
        // query a non-unique order can repeat or drop a product across pages.
        switch (sort)
        {
            case ShopSortOrder.PriceAscending:
                Query.OrderBy(p => p.Price).ThenBy(p => p.SortOrder).ThenBy(p => p.Id);
                break;
            case ShopSortOrder.PriceDescending:
                Query.OrderByDescending(p => p.Price).ThenBy(p => p.SortOrder).ThenBy(p => p.Id);
                break;
            case ShopSortOrder.Newest:
            default:
                Query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);
                break;
        }

        if (skip > 0)
            Query.Skip(skip);
        if (take is > 0)
            Query.Take(take.Value);
    }
}
