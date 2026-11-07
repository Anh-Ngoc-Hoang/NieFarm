namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// The write repository shares one scoped context, and in Blazor Server that scope is the whole
/// circuit — so an aggregate loaded earlier stays tracked, and later loads return the cached
/// (possibly stale) instance. Call <see cref="DiscardTrackedEntities"/> before loading an aggregate
/// whose current state decides a write, so the decision is made against the database.
/// </summary>
public interface IWriteContextRefresher
{
    /// <summary>
    /// Detaches everything the shared write context tracks. Throws if there are unsaved changes,
    /// since that indicates a programming error.
    /// </summary>
    void DiscardTrackedEntities();
}
