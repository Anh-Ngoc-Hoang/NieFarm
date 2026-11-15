namespace NieFarm.Application.Features.Articles.Dtos;

/// <summary>
/// Listing / related-list card. Never carries the body. <see cref="ImageUrl"/> is null when the
/// article has neither a card image nor a hero image; <see cref="ImageAlt"/> is never blank.
/// </summary>
public record ArticleCardDto(
    int Id,
    string Slug,
    string Title,
    string CategoryName,
    DateOnly PublishedOn,
    string Summary,
    string? ImageUrl,
    string ImageAlt);
