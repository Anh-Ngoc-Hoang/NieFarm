namespace NieFarm.Application.Features.Products.Dtos;

/// <summary>ValueLabels is positional: one entry per ProductDetailDto.Options[i], in option SortOrder.</summary>
public record ProductVariantDto(IReadOnlyList<string> ValueLabels, decimal Price, string? ImageUrl, bool IsAvailable);
