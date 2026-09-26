using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

public class Product : BaseEntity
{
    private readonly List<ProductOption> _options = [];
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductSpec> _specs = [];
    private readonly List<ProductFeature> _features = [];
    private readonly List<ProductImage> _images = [];

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public ProductCategory? Category { get; private set; }

    /// <summary>Unit price in Vietnamese đồng.</summary>
    public decimal Price { get; private set; }

    /// <summary>Optional "was" price shown struck through next to <see cref="Price"/>. Display-only, no promotion semantics.</summary>
    public decimal? OldPrice { get; private set; }

    public string ImageUrl { get; private set; } = string.Empty;
    public string ImageAlt { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// <summary>Optional pill shown on the card, e.g. "Mới" or "Bán chạy".</summary>
    public string Badge { get; private set; } = string.Empty;

    public bool IsFeatured { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public int SortOrder { get; private set; }

    /// <summary>
    /// The axes this product varies along. Empty for a product sold as a single SKU, in which
    /// case <see cref="Price"/> is the only price.
    /// </summary>
    public IReadOnlyCollection<ProductOption> Options => _options.AsReadOnly();

    /// <summary>
    /// Every combination of <see cref="Options"/> values, each with its own price. Empty
    /// whenever <see cref="Options"/> is empty.
    /// </summary>
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    /// <summary>Specification rows, in display order. Rendered as "Thông số kỹ thuật".</summary>
    public IReadOnlyCollection<ProductSpec> Specs => _specs.AsReadOnly();

    /// <summary>Highlight bullets, in display order. Rendered as "Đặc điểm nổi bật".</summary>
    public IReadOnlyCollection<ProductFeature> Features => _features.AsReadOnly();

    /// <summary>
    /// Additional gallery images for the product detail page, in display order. This is
    /// additive to <see cref="ImageUrl"/>, which remains the card/cart-facing primary image.
    /// </summary>
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private Product() { }

    public static Product Create(
        string name, string slug, int categoryId, decimal price, decimal? oldPrice, string imageUrl, string imageAlt,
        string summary, string description, string badge, bool isFeatured, bool isVisible, int sortOrder)
    {
        var product = new Product();
        product.Update(name, slug, categoryId, price, oldPrice, imageUrl, imageAlt, summary, description,
            badge, isFeatured, isVisible, sortOrder);
        return product;
    }

    public void Update(
        string name, string slug, int categoryId, decimal price, decimal? oldPrice, string imageUrl, string imageAlt,
        string summary, string description, string badge, bool isFeatured, bool isVisible, int sortOrder)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        CategoryId = categoryId;
        Price = price;
        OldPrice = oldPrice;
        ImageUrl = imageUrl.Trim();
        ImageAlt = imageAlt.Trim();
        Summary = summary.Trim();
        Description = description.Trim();
        Badge = badge.Trim();
        IsFeatured = isFeatured;
        IsVisible = isVisible;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }

    /// <summary>
    /// Replaces the specification table. A row blank in both fields is dropped — that is the
    /// editor's empty starter row, not input. A row filled in on only one side is kept: it is
    /// still something the admin typed.
    /// </summary>
    public void SetSpecs(IEnumerable<(string Label, string Value)> specs)
    {
        _specs.Clear();

        var order = 0;
        foreach (var (label, value) in specs)
        {
            if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(value))
                continue;

            _specs.Add(new ProductSpec(label ?? string.Empty, value ?? string.Empty, order++));
        }

        SetUpdatedAt();
    }

    /// <summary>Replaces the highlight bullets, dropping blank entries.</summary>
    public void SetFeatures(IEnumerable<string> features)
    {
        _features.Clear();

        var order = 0;
        foreach (var text in features)
        {
            if (string.IsNullOrWhiteSpace(text))
                continue;

            _features.Add(new ProductFeature(text, order++));
        }

        SetUpdatedAt();
    }

    /// <summary>Replaces the gallery, dropping blank URLs. Ordering is the order supplied.</summary>
    public void SetImages(IEnumerable<string> urls)
    {
        _images.Clear();
        var order = 0;
        foreach (var url in urls)
        {
            if (string.IsNullOrWhiteSpace(url))
                continue;
            _images.Add(new ProductImage(url, order++));
        }
        SetUpdatedAt();
    }

    /// <summary>
    /// Clears and rebuilds the whole options/variants matrix. Options and variants must stay
    /// mutually consistent, so they are only ever set together.
    /// </summary>
    /// <param name="options">Each axis and its values, in display order.</param>
    /// <param name="variants">
    /// One entry per combination. <c>ValueIndexes</c> addresses values by <b>position</b> —
    /// <c>ValueIndexes[i]</c> is an index into <c>options[i].Values</c> — because the rows are
    /// deleted and re-inserted on every save, so their Ids are not stable enough to reference.
    /// </param>
    public void SetOptionsAndVariants(
        IReadOnlyList<(string Name, IReadOnlyList<string> Values)> options,
        IReadOnlyList<(IReadOnlyList<int> ValueIndexes, decimal Price, string? ImageUrl,
            bool IsAvailable)> variants)
    {
        // Variants first, then options: EF has to see the join rows go before the values they
        // point at, since that FK is ClientCascade rather than a database-level cascade.
        _variants.Clear();
        _options.Clear();

        var normalisedOptions = options
            .Select(o => (
                Name: o.Name.Trim(),
                Values: o.Values.Select(v => v.Trim()).Where(v => v.Length > 0).ToList()))
            .Where(o => o.Name.Length > 0 && o.Values.Count > 0)
            .ToList();

        // No usable axis — the product falls back to its own Price and carries no variants.
        if (normalisedOptions.Count == 0)
        {
            SetUpdatedAt();
            return;
        }

        var order = 0;
        var valueLookup = new ProductOptionValue[normalisedOptions.Count][];
        foreach (var (name, values) in normalisedOptions)
        {
            var option = new ProductOption(name, order);
            var optionValues = new ProductOptionValue[values.Count];
            for (var valueOrder = 0; valueOrder < values.Count; valueOrder++)
                optionValues[valueOrder] = option.AddValue(values[valueOrder], valueOrder);

            valueLookup[order] = optionValues;
            _options.Add(option);
            order++;
        }

        // Callers reach here through a validator that reports these as messages; the guards
        // below are the last line of defence against a malformed matrix being persisted.
        var seenCombinations = new HashSet<string>();
        var variantOrder = 0;
        foreach (var (valueIndexes, price, imageUrl, isAvailable) in variants)
        {
            if (valueIndexes.Count != normalisedOptions.Count)
                throw new InvalidOperationException("Every variant must select exactly one value per option.");

            for (var i = 0; i < valueIndexes.Count; i++)
            {
                if (valueIndexes[i] < 0 || valueIndexes[i] >= valueLookup[i].Length)
                    throw new InvalidOperationException("Variant option value index is out of range.");
            }

            if (!seenCombinations.Add(string.Join('|', valueIndexes)))
                throw new InvalidOperationException("Duplicate variant combination.");

            if (price < 0)
                throw new InvalidOperationException("Variant price cannot be negative.");

            var variant = new ProductVariant(price, imageUrl, isAvailable, variantOrder++);
            for (var i = 0; i < valueIndexes.Count; i++)
                variant.AddValue(valueLookup[i][valueIndexes[i]]);

            _variants.Add(variant);
        }

        SetUpdatedAt();
    }
}
