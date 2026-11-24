using System.Text;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Application.Features.Products;
using NieFarm.Application.Features.Traceability;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Queries;

/// <summary>Two queries (batches, then the counters of those batches) joined in memory; the list is small.</summary>
public class GetRoastBatchesAdminQueryHandler(
    IReadRepositoryBase<RoastBatch> batches,
    IReadRepositoryBase<RoastBatchScanStat> stats)
    : IRequestHandler<GetRoastBatchesAdminQuery, List<RoastBatchAdminListDto>>
{
    public async Task<List<RoastBatchAdminListDto>> Handle(
        GetRoastBatchesAdminQuery request, CancellationToken cancellationToken)
    {
        var term = ProductSearchTerm.Normalize(request.Search);
        var list = await batches.ListAsync(new AllRoastBatchesAdminSpec(term, CodeTerm(term)), cancellationToken);
        if (list.Count == 0)
            return [];

        var counters = (await stats.ListAsync(
                new ScanStatsByBatchIdsSpec(list.Select(b => b.Id).ToList(), noTracking: true), cancellationToken))
            .ToDictionary(s => s.BatchId);

        return list.Select(b =>
        {
            counters.TryGetValue(b.Id, out var stat);
            var active = stat?.IsAlertActive == true;

            return new RoastBatchAdminListDto(
                b.Id,
                b.Code,
                BatchCode.Format(b.Code),
                b.Product?.Name ?? string.Empty,
                b.RoastDate,
                b.IsPublished,
                stat?.TotalScans ?? 0,
                stat is not null && stat.DayVn == request.TodayVn ? stat.DayScans : 0,
                stat?.LastScanAt,
                active,
                active ? stat!.AlertReason : null);
        }).ToList();
    }

    /// <summary>
    /// A code is typed with or without dashes in any case, so the code filter keeps only the characters a code
    /// can contain, upper-cased. Null when none are left (the term is then only matched against the product name).
    /// </summary>
    private static string? CodeTerm(string? term)
    {
        if (term is null)
            return null;

        var buffer = new StringBuilder(term.Length);
        foreach (var c in term)
        {
            var upper = c is >= 'a' and <= 'z' ? (char)(c - 32) : c;
            if (BatchCode.Alphabet.IndexOf(upper) >= 0)
                buffer.Append(upper);
        }

        return buffer.Length == 0 ? null : buffer.ToString();
    }
}
