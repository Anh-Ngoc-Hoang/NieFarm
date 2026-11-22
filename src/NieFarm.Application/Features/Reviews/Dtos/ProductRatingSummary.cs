namespace NieFarm.Application.Features.Reviews.Dtos;

/// <summary>Average (one decimal) and count of a product's Approved reviews. Count is always at least 1.</summary>
public record ProductRatingSummary(double AverageRating, int ReviewCount);
