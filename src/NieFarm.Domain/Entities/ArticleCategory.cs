using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

public class ArticleCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsVisible { get; private set; } = true;
    public int SortOrder { get; private set; }

    private ArticleCategory() { }

    public static ArticleCategory Create(string name, string slug, bool isVisible, int sortOrder)
    {
        var category = new ArticleCategory();
        category.Update(name, slug, isVisible, sortOrder);
        return category;
    }

    public void Update(string name, string slug, bool isVisible, int sortOrder)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        IsVisible = isVisible;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }
}
