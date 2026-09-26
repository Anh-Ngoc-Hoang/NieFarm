namespace NieFarm.Application.Features.Reviews.Dtos;

public record ProductReviewsDto(double AverageRating, int ApprovedCount, IReadOnlyList<ProductReviewDto> Reviews);
