using MediatR;
using NieFarm.Application.Features.Traceability.Dtos;

namespace NieFarm.Application.Features.Traceability.Queries;

/// <summary>
/// The public traceability page's data. Returns <c>null</c> (not a Result) for every "no such page" case, wrong
/// shape, unknown and unpublished alike (R-26 D4): a Result.NotFound would make LoggingBehavior write a Warning
/// for every wrong code a visitor or a scanner tries. There is deliberately no validator for the same reason (a
/// validator on a query that returns a plain DTO makes ValidationBehavior throw).
/// </summary>
/// <param name="IncludeUnpublished">True only for an admin preview; such a read is never cached.</param>
public record GetBatchTraceQuery(string Code, bool IncludeUnpublished = false) : IRequest<BatchTraceDto?>;
