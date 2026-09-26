namespace NieFarm.Application.Features.Products.Dtos;

/// <summary>The "Danh Mục" filter checkboxes on the shop page. Deliberately minimal — no counts.</summary>
public record ProductCategoryFilterDto(int Id, string Name);
