namespace NieFarm.Application.Features.Reviews.Dtos;

/// <summary>
/// <paramref name="IsPending"/> is only ever true for the caller's own review — the spec
/// (<see cref="NieFarm.Application.Features.Reviews.Specifications.ProductReviewsVisibleToSpec"/>)
/// guarantees no other pending row can reach this DTO.
/// <paramref name="IsVerifiedPurchase"/> (R-17) is the only purchase-derived fact exposed publicly: a
/// bare flag. This DTO deliberately carries no author user id, order id, order code, date or quantity.
/// </summary>
public record ProductReviewDto(
    int Id,
    string AuthorName,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    bool IsPending,
    bool IsVerifiedPurchase);
