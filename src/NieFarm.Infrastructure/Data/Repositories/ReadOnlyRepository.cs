using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NieFarm.Infrastructure.Data.Repositories;

/// <summary>
/// Read-only repository backed by a fresh <see cref="AppDbContext"/> per instance
/// (registered transient), so read queries are isolated and safe to run in parallel.
/// Entities loaded here are tracked by a different context — never hand them to
/// <see cref="Repository{T}"/> for update or delete.
/// </summary>
internal sealed class ReadOnlyRepository<T> : RepositoryBase<T>, IAsyncDisposable
    where T : class
{
    private readonly AppDbContext _context;

    public ReadOnlyRepository(IDbContextFactory<AppDbContext> factory)
        : this(factory.CreateDbContext()) { }

    private ReadOnlyRepository(AppDbContext context) : base(context)
        => _context = context;

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
