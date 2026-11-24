using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Data;

internal sealed class WriteContextRefresher(AppDbContext db) : IWriteContextRefresher
{
    public void DiscardTrackedEntities()
    {
        if (db.ChangeTracker.HasChanges())
            throw new InvalidOperationException(
                "Cannot discard tracked entities while the write context has unsaved changes.");

        db.ChangeTracker.Clear();
    }

    public void ForceDiscardTrackedEntities() => db.ChangeTracker.Clear();
}
