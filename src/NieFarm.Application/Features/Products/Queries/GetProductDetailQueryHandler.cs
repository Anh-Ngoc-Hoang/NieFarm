using System.Net;
using System.Text.RegularExpressions;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Queries;

public partial class GetProductDetailQueryHandler(IReadRepositoryBase<Product> repository)
    : IRequestHandler<GetProductDetailQuery, ProductDetailDto?>
{
    public async Task<ProductDetailDto?> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(
            new ProductDetailBySlugSpec(request.Slug), cancellationToken);
        if (product is null)
            return null;

        var gallery = BuildGallery(product);

        var options = ProductVariantResolver.BuildOptions(product);
        var variants = ProductVariantResolver.BuildVariants(product);

        var specs = product.Specs.OrderBy(s => s.SortOrder)
            .Select(s => new ProductSpecDto(s.Label, s.Value))
            .ToList();

        var features = product.Features.OrderBy(f => f.SortOrder)
            .Select(f => f.Text)
            .ToList();

        return new ProductDetailDto(
            product.Id,
            product.Slug,
            product.Name,
            product.Category?.Name ?? string.Empty,
            product.Price,
            product.OldPrice,
            string.IsNullOrWhiteSpace(product.Badge) ? null : product.Badge,
            product.Summary,
            ToPlainText(product.Summary),
            product.Description,
            gallery,
            options,
            variants,
            specs,
            features);
    }

    /// <summary>
    /// Product.ImageUrl first (when non-blank), then the additive gallery ordered by
    /// SortOrder, de-duplicated case-insensitively — an admin may paste the primary URL into
    /// the gallery too. Alt text falls back to Product.ImageAlt, or Product.Name when blank.
    /// </summary>
    private static List<ProductImageDto> BuildGallery(Product product)
    {
        var alt = string.IsNullOrWhiteSpace(product.ImageAlt) ? product.Name : product.ImageAlt;

        var urls = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(product.ImageUrl) && seen.Add(product.ImageUrl))
            urls.Add(product.ImageUrl);

        foreach (var image in product.Images.OrderBy(i => i.SortOrder))
        {
            if (seen.Add(image.Url))
                urls.Add(image.Url);
        }

        return urls.Select(url => new ProductImageDto(url, alt)).ToList();
    }

    /// <summary>Strips tags, decodes entities, collapses whitespace and truncates for &lt;meta name="description"&gt;.</summary>
    private static string ToPlainText(string html)
    {
        var stripped = TagRegex().Replace(html, " ");
        var decoded = WebUtility.HtmlDecode(stripped);
        var collapsed = WhitespaceRegex().Replace(decoded, " ").Trim();

        const int maxLength = 160;
        return collapsed.Length <= maxLength ? collapsed : collapsed[..maxLength].TrimEnd() + "…";
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
