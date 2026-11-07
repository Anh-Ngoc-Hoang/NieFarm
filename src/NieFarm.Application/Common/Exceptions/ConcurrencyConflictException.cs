namespace NieFarm.Application.Common.Exceptions;

/// <summary>
/// Thrown by the write repository when a row was changed by someone else between load and save
/// (optimistic concurrency). Application-owned so handlers never reference EF Core types.
/// </summary>
public sealed class ConcurrencyConflictException(string entityName, Exception? inner = null)
    : Exception($"The {entityName} was modified by someone else.", inner)
{
    public string EntityName { get; } = entityName;
}
