namespace NieFarm.Application.Features.Reviews.Dtos;

/// <summary>One approved review's rating, projected so the shop card aggregate never loads comments.</summary>
public record ProductRatingRow(int ProductId, int Rating);
