namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>
/// Caps on a product's specification and highlight lists, enforced in the admin form and again
/// in the validator. Lengths match the column widths in the EF configuration.
/// </summary>
public static class ProductSpecLimits
{
    public const int MaxSpecs = 30;
    public const int MaxFeatures = 30;
    public const int MaxLabelLength = 150;
    public const int MaxValueLength = 300;
    public const int MaxFeatureLength = 500;
}
