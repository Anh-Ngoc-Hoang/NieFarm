using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

public record ApproveReviewCommand(int Id) : IRequest<Result>;
