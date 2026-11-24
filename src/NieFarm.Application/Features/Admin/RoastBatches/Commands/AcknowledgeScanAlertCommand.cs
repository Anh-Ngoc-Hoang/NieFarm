using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

/// <summary>Closes the open scan alert of a batch. Idempotent: no open alert is a success.</summary>
public record AcknowledgeScanAlertCommand(int BatchId) : IRequest<Result>;
