using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NieFarm.Application.Common.Exceptions;

namespace NieFarm.Infrastructure.Data.Repositories;

/// <summary>
/// Read-write repository over the shared scoped <see cref="AppDbContext"/>.
/// Writes through this type must always be awaited sequentially — see
/// .claude/rules/data-access.md.
/// </summary>
public class Repository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext) where T : class
{
    /// <summary>
    /// Every Add/Update/Delete on <see cref="RepositoryBase{T}"/> funnels through here. A
    /// concurrency failure is translated to <see cref="ConcurrencyConflictException"/> (Application
    /// has no EF reference) and the tracker is cleared: the context is circuit-scoped, the failed
    /// unit of work was rolled back, and leaving its Modified/Added entries tracked would make every
    /// later write from this circuit throw again.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            dbContext.ChangeTracker.Clear();
            throw new ConcurrencyConflictException(typeof(T).Name, ex);
        }
        catch (DbUpdateException)
        {
            // Timeout, deadlock, constraint failure: the transaction rolled back, but the failed
            // entities would stay tracked and make IWriteContextRefresher throw on every later
            // write from this circuit. Clear them and let the original exception surface.
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
