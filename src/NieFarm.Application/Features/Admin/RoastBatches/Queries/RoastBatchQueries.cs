using MediatR;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;

namespace NieFarm.Application.Features.Admin.RoastBatches.Queries;

/// <summary>
/// The admin batch list. <paramref name="TodayVn"/> is today in Vietnam time (the Application does not know the
/// zone, so the web layer passes it); a counter row counts as "today" only when its day equals it.
/// </summary>
public record GetRoastBatchesAdminQuery(string? Search, DateOnly TodayVn) : IRequest<List<RoastBatchAdminListDto>>;

/// <summary>Null when the batch does not exist.</summary>
public record GetRoastBatchForEditQuery(int Id) : IRequest<RoastBatchEditDto?>;

/// <summary>Null when the batch does not exist. Works for unpublished batches too.</summary>
public record GetRoastBatchLabelQuery(int Id) : IRequest<RoastBatchLabelDto?>;

/// <summary>How many batches have an open scan alert (the sidebar badge).</summary>
public record GetActiveScanAlertCountQuery : IRequest<int>;
