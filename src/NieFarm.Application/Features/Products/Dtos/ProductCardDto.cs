namespace NieFarm.Application.Features.Products.Dtos;

/// <summary>
/// The fields a product grid card needs — used by <c>/cua-hang</c> and the "Sản phẩm liên
/// quan" band on <c>/san-pham/{slug}</c>. <see cref="Summary"/> is shown only by the latter.
/// </summary>
public record ProductCardDto(
    string Slug,
    string Name,
    string CategoryName,
    decimal Price,
    string ImageUrl,
    string ImageAlt,
    string? Badge,
    string Summary);
