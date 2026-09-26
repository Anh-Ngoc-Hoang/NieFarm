using Ardalis.Specification.EntityFrameworkCore;

namespace NieFarm.Infrastructure.Data.Repositories;

/// <summary>
/// Read-write repository over the shared scoped <see cref="AppDbContext"/>.
/// Writes through this type must always be awaited sequentially — see
/// .claude/rules/data-access.md.
/// </summary>
public class Repository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext) where T : class;
