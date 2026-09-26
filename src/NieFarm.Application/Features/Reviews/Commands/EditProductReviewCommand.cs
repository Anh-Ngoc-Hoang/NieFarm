using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Reviews.Commands;

/// <summary>
/// No <c>ProductId</c>, no user id, no author name, no status — <c>ProductId</c> is deliberately
/// absent so an edit can never relocate a review to a different product.
/// </summary>
public record EditProductReviewCommand(int ReviewId, int Rating, string Comment) : IRequest<Result>;
