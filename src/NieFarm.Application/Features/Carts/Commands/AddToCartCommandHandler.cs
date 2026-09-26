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

public class AddToCartCommandHandler(
    IRepositoryBase<DomainCart> cartRepository,
    IReadRepositoryBase<Product> productRepository,
    CartAssembler assembler,
    ICurrentUser currentUser)
    : IRequestHandler<AddToCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return Result<CartDto>.Error("Không xác định được giỏ hàng.");

        // Read repo, entity used for values only, never handed to the write repo (data-access.md).
        var product = await productRepository.FirstOrDefaultAsync(
            new ProductByIdForCartSpec(request.ProductId), cancellationToken);
        if (product is null)
            return Result<CartDto>.NotFound("Không tìm thấy sản phẩm.");

        var labels = ProductVariantResolver.Canonicalise(product, request.SelectedOptionValues);
        if (labels is null)
            return Result<CartDto>.Error("Vui lòng chọn phiên bản sản phẩm hợp lệ.");

        if (product.Options.Count > 0)
        {
            var variant = ProductVariantResolver.FindVariant(product, labels);
            if (variant is null)
                return Result<CartDto>.Error("Vui lòng chọn phiên bản sản phẩm hợp lệ.");

            // The server-side re-check that makes the UI's IsOutOfStock disable cosmetic
            // rather than load-bearing.
            if (!variant.IsAvailable)
                return Result<CartDto>.Error("Sản phẩm hiện tạm hết hàng.");
        }

        var variantKey = CartLineKey.From(labels);

        // Load-or-create through the write repo — never mix a read-repo entity into a write.
        var cart = await cartRepository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);

        if (cart is null)
        {
            cart = !string.IsNullOrWhiteSpace(userId)
                ? DomainCart.ForUser(userId)
                : DomainCart.ForAnonymous(request.AnonymousId!);

            cart.AddOrIncrement(product.Id, variantKey, request.Quantity);
            await cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            if (cart.IsFullFor(product.Id, variantKey))
                return Result<CartDto>.Error(
                    $"Giỏ hàng chỉ chứa được tối đa {DomainCart.MaxDistinctItems} sản phẩm khác nhau.");

            cart.AddOrIncrement(product.Id, variantKey, request.Quantity);
            await cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return Result<CartDto>.Success(await assembler.BuildAsync(cart, cancellationToken));
    }
}
