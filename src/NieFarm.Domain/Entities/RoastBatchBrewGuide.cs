using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A suggested recipe for one <see cref="BrewMethod"/> on a <see cref="RoastBatch"/>; at most one per
/// method per batch. Owned by the aggregate root; only <see cref="RoastBatch.SetBrewGuides"/> creates,
/// updates or removes these.
/// </summary>
public class RoastBatchBrewGuide : BaseEntity
{
    public const int RatioMaxLength = 30;
    public const int BrewTimeMaxLength = 30;
    public const int NoteMaxLength = 200;
    public const int MaxWaterTempCelsius = 100;

    public int RoastBatchId { get; private set; }
    public BrewMethod Method { get; private set; }
    public string Ratio { get; private set; } = string.Empty;
    public int? WaterTempCelsius { get; private set; }
    public string BrewTime { get; private set; } = string.Empty;
    public string Note { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private RoastBatchBrewGuide() { }

    internal RoastBatchBrewGuide(BrewMethod method)
    {
        Method = method;
    }

    internal void Set(string ratio, int? waterTempCelsius, string brewTime, string note, int sortOrder)
    {
        Ratio = ratio;
        WaterTempCelsius = waterTempCelsius;
        BrewTime = brewTime;
        Note = note;
        SortOrder = sortOrder;
    }
}
