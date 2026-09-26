using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>
/// One review, but only if it belongs to this author. Ownership is part of the WHERE clause
/// rather than a post-load check — mirrors <c>MyOrderByIdSpec</c>. Used through the write
/// repository (<see cref="Ardalis.Specification.IRepositoryBase{T}"/>): the caller is about to
/// mutate the loaded entity, so it must be loaded and saved on the same tracked context — see
/// .claude/rules/data-access.md.
/// </summary>
public sealed class MyReviewByIdSpec : Specification<Review>, ISingleResultSpecification<Review>
{
    public MyReviewByIdSpec(int reviewId, string authorUserId)
    {
        Query.Where(r => r.Id == reviewId && r.AuthorUserId == authorUserId);
    }
}
