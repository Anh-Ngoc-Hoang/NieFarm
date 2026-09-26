namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>
/// Round-trips the admin form: <see cref="Options"/> and <see cref="Variants"/> reuse the
/// command's input records, so the shape read back is the shape submitted.
/// </summary>
public record ProductEditDto(
    int Id,
    string Name,
    string Slug,
    int CategoryId,
    decimal Price,
    decimal? OldPrice,
    string ImageUrl,
    string ImageAlt,
    string Summary,
    string Description,
    string Badge,
    bool IsFeatured,
    bool IsVisible,
    int SortOrder,
    List<ProductOptionInput> Options,
    List<ProductVariantInput> Variants,
    List<ProductSpecInput> Specs,
    List<string> Features,
    List<string> GalleryImageUrls);
