using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.ProductCategories.Commands;

public class DeleteProductCategoryCommandHandler(
    IRepositoryBase<ProductCategory> repository,
    IReadRepositoryBase<Product> products)
    : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.NotFound();

        var inUse = await products.AnyAsync(cancellationToken);
        if (inUse)
        {
            var all = await products.ListAsync(cancellationToken);
            var count = all.Count(p => p.CategoryId == request.Id);
            if (count > 0)
                return Result.Error($"This category still has {count} product(s). Move or delete them first.");
        }

        await repository.DeleteAsync(category, cancellationToken);
        return Result.Success();
    }
}
