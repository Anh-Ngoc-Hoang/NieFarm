namespace NieFarm.Application.Features.Products.Dtos;

public record ProductOptionDto(string Name, IReadOnlyList<string> Values);
