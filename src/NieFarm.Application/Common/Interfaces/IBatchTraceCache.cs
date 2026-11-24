using NieFarm.Application.Features.Traceability.Dtos;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Short-lived (60 s) cache of a published batch's public page data, keyed by the normalised 12-character code
/// (R-26 D15). Only found, published results are ever stored, never a "not found" and never an admin preview, so
/// a flood of random codes cannot fill memory.
/// </summary>
public interface IBatchTraceCache
{
    bool TryGet(string code, out BatchTraceDto? dto);

    void Set(string code, BatchTraceDto dto);

    /// <summary>Called when a batch is saved or deleted so an unpublish takes effect at once.</summary>
    void Invalidate(string code);
}
