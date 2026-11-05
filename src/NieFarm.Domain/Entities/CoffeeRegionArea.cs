using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One well-known coffee area inside a <see cref="CoffeeRegion"/> — a name and a short note, in
/// display order. Owned by the aggregate root; only <see cref="CoffeeRegion.SetAreas"/> creates,
/// updates, reorders or removes these, mirroring CropProduct on <see cref="Crop"/>.
/// </summary>
public class CoffeeRegionArea : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int NoteMaxLength = 300;

    public int CoffeeRegionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Note { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private CoffeeRegionArea() { }

    internal CoffeeRegionArea(string name, string note, int sortOrder)
    {
        Name = name;
        Note = note;
        SortOrder = sortOrder;
    }

    internal void Update(string name, string note, int sortOrder)
    {
        Name = name;
        Note = note;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }
}
