namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>
/// One combination as the admin form submits it. <paramref name="ValueIndexes"/> holds one
/// index per option, in option order — <c>ValueIndexes[i]</c> indexes into the i-th option's
/// <see cref="ProductOptionInput.Values"/>.
/// </summary>
public record ProductVariantInput(
    List<int> ValueIndexes,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable);
