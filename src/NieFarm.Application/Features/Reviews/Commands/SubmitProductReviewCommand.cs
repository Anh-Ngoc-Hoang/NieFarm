using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Reviews.Commands;

/// <summary>
/// No user id, no author name, no status — the browser supplies none of those; the handler
/// resolves the author from <see cref="NieFarm.Application.Common.Interfaces.ICurrentUser"/>.
/// </summary>
public record SubmitProductReviewCommand(int ProductId, int Rating, string Comment) : IRequest<Result<int>>;
