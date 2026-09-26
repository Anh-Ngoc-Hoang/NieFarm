using MediatR;
using NieFarm.Application.Features.Reviews.Dtos;

namespace NieFarm.Application.Features.Reviews.Queries;

public record GetProductReviewsQuery(int ProductId) : IRequest<ProductReviewsDto>;
