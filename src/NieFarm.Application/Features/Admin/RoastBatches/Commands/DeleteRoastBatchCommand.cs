using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

/// <summary>Deletes a batch that is not published. A published batch has printed labels in the field.</summary>
public record DeleteRoastBatchCommand(int Id) : IRequest<Result>;
