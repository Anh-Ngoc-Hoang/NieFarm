namespace NieFarm.Application.Features.Admin.ArticleCategories.Dtos;

public record ArticleCategoryDto(
    int Id,
    string Name,
    string Slug,
    bool IsVisible,
    int SortOrder,
    int ArticleCount);
