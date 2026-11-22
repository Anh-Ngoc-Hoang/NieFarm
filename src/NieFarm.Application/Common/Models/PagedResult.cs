namespace NieFarm.Application.Common.Models;

/// <summary>
/// One page of a larger, server-paged result. <see cref="Page"/> and <see cref="PageSize"/> are the
/// values the server actually used (after clamping), not what the caller asked for. The record must
/// stay JSON-serialisable (Shop's [PersistentState]).
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
}
