using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Crops.Specifications;

/// <summary>
/// Every row with IsCurrent set (normally 0 or 1 — the filtered unique index in
/// CropConfiguration enforces at most one). Used by SetCurrentCropCommandHandler's first step to
/// clear every current row other than the target (D1).
/// </summary>
public sealed class CurrentCropsSpec : Specification<Crop>
{
    public CurrentCropsSpec()
    {
        Query.Where(c => c.IsCurrent);
    }
}
