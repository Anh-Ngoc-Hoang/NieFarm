using MediatR;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Products.Queries;

/// <summary>
/// Everything /san-pham/{slug} needs to render. Null means not found or not visible — "not
/// found" is the only failure mode here, so it does not need a Result{T}, consistent with
/// GetProductForEditQuery.
/// </summary>
public record GetProductDetailQuery(string Slug) : IRequest<ProductDetailDto?>;
