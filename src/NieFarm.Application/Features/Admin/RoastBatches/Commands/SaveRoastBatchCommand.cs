using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

/// <summary>
/// Creates when <paramref name="Id"/> is null (the system generates the code), otherwise updates that row (the
/// code never changes: it is printed). Returns the batch id.
/// </summary>
public record SaveRoastBatchCommand(
    int? Id,
    int ProductId,
    bool IsPublished,
    RoastBatchGeneralInput General,
    RoastBatchOriginInput Origin,
    RoastBatchProcessInput Process,
    RoastBatchQualityInput Quality,
    RoastBatchBrewingInput Brewing) : IRequest<Result<int>>;
