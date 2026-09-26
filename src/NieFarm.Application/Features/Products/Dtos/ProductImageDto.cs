namespace NieFarm.Application.Features.Products.Dtos;

/// <summary>Alt is inherited from Product.ImageAlt (or Product.Name when blank) — ProductImage has no alt column by design.</summary>
public record ProductImageDto(string Url, string Alt);
