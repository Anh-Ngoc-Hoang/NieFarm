using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Application.Features.Products;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts.Commands;

public class SetCartLineQuantityCommandHandler(
    IRepositoryBase<DomainCart> repository,
    IReadRepositoryBase<Product> productRepository,
    CartAssembler assembler,
    ICurrentUser currentUser)
    : IRequestHandler<SetCartLineQuantityCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(SetCartLineQuantityCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.FirstOrDefaultAsync(
            new ProductByIdForCartSpec(request.ProductId), cancellationToken);
        if (product is null)
            return Result<CartDto>.NotFound("Sản phẩm không có trong giỏ hàng.");

        var labels = CartLineKey.Split(request.VariantKey);

        // Deliberate nuance: only checking the combination still resolves to a real variant, not
        // whether it is available — a shopper reducing the quantity of, or undoing the removal
        // of, a line that has just gone out of stock must not be blocked here. D9 already blocks
        // the order itself.
        if (product.Options.Count > 0 && ProductVariantResolver.FindVariant(product, labels) is null)
            return Result<CartDto>.NotFound("Sản phẩm không có trong giỏ hàng.");

        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return Result<CartDto>.Error("Không xác định được giỏ hàng.");

        // Load-or-create through the write repo — never mix a read-repo entity into a write.
        var cart = await repository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);

        if (cart is null)
        {
            cart = !string.IsNullOrWhiteSpace(userId)
                ? DomainCart.ForUser(userId)
                : DomainCart.ForAnonymous(request.AnonymousId!);

            cart.AddOrIncrement(request.ProductId, request.VariantKey, request.Quantity);
            await repository.AddAsync(cart, cancellationToken);
        }
        else
        {
            var hasLine = cart.Items.Any(i => i.ProductId == request.ProductId && i.VariantKey == request.VariantKey);
            if (!hasLine)
            {
                if (cart.IsFullFor(request.ProductId, request.VariantKey))
                    return Result<CartDto>.Error(
                        $"Giỏ hàng chỉ chứa được tối đa {DomainCart.MaxDistinctItems} sản phẩm khác nhau.");

                cart.AddOrIncrement(request.ProductId, request.VariantKey, request.Quantity);
            }
            else
            {
                cart.SetQuantity(request.ProductId, request.VariantKey, request.Quantity);
            }

            await repository.UpdateAsync(cart, cancellationToken);
        }

        return Result<CartDto>.Success(await assembler.BuildAsync(cart, cancellationToken));
    }
}
