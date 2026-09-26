using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

public record DeleteReviewCommand(int Id) : IRequest<Result>;
