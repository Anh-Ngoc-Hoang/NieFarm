using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

/// <summary>
/// No Rating, no Status, no ProductId, no author fields, no editor id — the editing admin is
/// resolved server-side from ICurrentUser. Id (not ReviewId) to match the sibling
/// ApproveReviewCommand(int Id)/RejectReviewCommand(int Id)/DeleteReviewCommand(int Id) on this
/// admin surface.
/// </summary>
public record EditReviewCommentCommand(int Id, string Comment) : IRequest<Result>;
