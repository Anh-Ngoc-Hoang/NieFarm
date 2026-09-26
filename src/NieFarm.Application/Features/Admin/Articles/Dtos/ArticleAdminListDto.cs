namespace NieFarm.Application.Features.Admin.Articles.Dtos;

public record ArticleAdminListDto(
    int Id,
    string Title,
    string Slug,
    string CategoryName,
    string CategorySlug,
    string Author,
    string ImageUrl,
    DateOnly PublishedOn,
    bool IsPublished);
