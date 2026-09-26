using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Admin.Products.Dtos;

namespace NieFarm.Application.Features.Admin.Products.Commands;

/// <summary>
/// Creates when <paramref name="Id"/> is null, otherwise updates that row.
/// A blank <paramref name="Slug"/> is generated from the name.
/// <paramref name="Options"/> and <paramref name="Variants"/> replace the product's matrix
/// wholesale; both empty means the product is sold as a single SKU at <paramref name="Price"/>.
/// </summary>
public record SaveProductCommand(
    int? Id,
    string Name,
    string? Slug,
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
    List<string> GalleryImageUrls) : IRequest<Result<int>>;
