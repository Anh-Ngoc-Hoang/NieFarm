using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

public record RejectReviewCommand(int Id) : IRequest<Result>;
