namespace NieFarm.Application.Features.Admin.Articles.Dtos;

public record ArticleEditDto(
    int Id,
    string Title,
    string Slug,
    int CategoryId,
    string Author,
    string Summary,
    string ImageUrl,
    string ImageAlt,
    string HeroImageUrl,
    string HeroImageAlt,
    string Body,
    DateOnly PublishedOn,
    bool IsPublished);
