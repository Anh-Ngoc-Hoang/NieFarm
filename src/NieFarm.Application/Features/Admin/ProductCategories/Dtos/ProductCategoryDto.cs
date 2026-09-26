namespace NieFarm.Application.Features.Admin.ProductCategories.Dtos;

public record ProductCategoryDto(
    int Id,
    string Name,
    string Slug,
    bool IsVisible,
    int SortOrder,
    int ProductCount);
