using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One certification (e.g. "VietGAP", "Organic") claimed for a <see cref="RoastBatch"/>. Owned by the
/// aggregate root; only <see cref="RoastBatch.SetCertifications"/> creates or removes these.
/// </summary>
public class RoastBatchCertification : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int NumberMaxLength = 100;

    public int RoastBatchId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional certificate number; <c>""</c> means none.</summary>
    public string Number { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    private RoastBatchCertification() { }

    internal RoastBatchCertification(string name, string number, int sortOrder)
    {
        Name = name;
        Number = number;
        SortOrder = sortOrder;
    }
}
