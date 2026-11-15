using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Addresses.Queries;
using NieFarm.Application.Features.Shipping;

namespace NieFarm.Application.Features.Admin.Shipping.Queries;

/// <summary>
/// Drift detection: lists the names the address service returns that do not resolve to the
/// in-code province catalog (those customers would pay the base fee). An unreachable service is an
/// error result (nothing to compare), not an empty list.
/// </summary>
public record CheckProvinceCatalogQuery : IRequest<Result<IReadOnlyList<string>>>;

public class CheckProvinceCatalogQueryHandler(ISender sender)
    : IRequestHandler<CheckProvinceCatalogQuery, Result<IReadOnlyList<string>>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        CheckProvinceCatalogQuery request, CancellationToken cancellationToken)
    {
        var provinces = await sender.Send(new GetProvincesQuery(), cancellationToken);
        if (!provinces.IsSuccess)
            return Result<IReadOnlyList<string>>.Error("The address service could not be reached.");

        IReadOnlyList<string> unmatched = provinces.Value
            .Where(p => VietnamProvinceCatalog.TryResolve(p.Name) is null)
            .Select(p => p.Name)
            .ToList();

        return Result<IReadOnlyList<string>>.Success(unmatched);
    }
}
