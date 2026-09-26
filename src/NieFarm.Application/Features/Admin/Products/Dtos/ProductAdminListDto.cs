namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>
/// A catalog row for the admin list. <see cref="Price"/> is the product's own base price —
/// the admin sees the stored value, not an effective one. <see cref="MinPrice"/> /
/// <see cref="MaxPrice"/> are null unless the product has variants.
/// </summary>
public record ProductAdminListDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string CategorySlug,
    decimal Price,
    string ImageUrl,
    string Badge,
    bool IsFeatured,
    bool IsVisible,
    int SortOrder,
    decimal? MinPrice,
    decimal? MaxPrice,
    int VariantCount);
