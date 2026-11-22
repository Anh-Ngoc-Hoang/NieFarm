namespace NieFarm.Application.Features.Products.Dtos;

/// <summary>
/// The fields a product grid card needs — used by <c>/cua-hang</c>, the home featured band and
/// "shop the crop" list, the "Sản phẩm liên quan" band on <c>/san-pham/{slug}</c> and the
/// <c>ArticleDetail</c> sidebar. <c>ProductCard.razor</c> does not display <see cref="Summary"/>.
/// <see cref="Id"/>, <see cref="HasOptions"/> and <see cref="IsAvailable"/> (appended last) drive the
/// card's add-to-cart button; the record must stay JSON-serialisable (Home's [PersistentState]).
/// <see cref="AverageRating"/> / <see cref="ReviewCount"/> (appended last, so an older persisted JSON
/// still deserialises) are the Approved-review summary; <see cref="ReviewCount"/> 0 means "no rating
/// yet" and the card hides the row. Cards built without a lookup (the crop list) carry none.
/// </summary>
public record ProductCardDto(
    string Slug,
    string Name,
    string CategoryName,
    decimal Price,
    string ImageUrl,
    string ImageAlt,
    string? Badge,
    string Summary,
    int Id,
    bool HasOptions,
    bool IsAvailable,
    double? AverageRating = null,
    int ReviewCount = 0);
