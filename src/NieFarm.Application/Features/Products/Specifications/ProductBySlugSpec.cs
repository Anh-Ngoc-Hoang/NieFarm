using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

public sealed class ProductBySlugSpec : Specification<Product>, ISingleResultSpecification<Product>
{
    public ProductBySlugSpec(string slug, int? excludeId = null)
    {
        Query.Where(p => p.Slug == slug);

        if (excludeId is { } id)
            Query.Where(p => p.Id != id);
    }
}
