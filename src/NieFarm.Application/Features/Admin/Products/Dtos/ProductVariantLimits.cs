namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>
/// Caps on the size of a product's option matrix. Enforced in the admin form (so the editor
/// stays usable) and again in the validator (so the API cannot be talked past it).
/// </summary>
public static class ProductVariantLimits
{
    public const int MaxOptions = 3;
    public const int MaxValuesPerOption = 12;
    public const int MaxVariants = 60;
    public const int MaxOptionNameLength = 100;
    public const int MaxValueLabelLength = 100;
}
