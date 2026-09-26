using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Commands;

public class SetCartLineQuantityCommandValidator : AbstractValidator<SetCartLineQuantityCommand>
{
    public SetCartLineQuantityCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Sản phẩm không có trong giỏ hàng.");

        RuleFor(x => x.VariantKey)
            .MaximumLength(CartLineKey.MaxLength);

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, Cart.MaxQuantityPerItem)
            .WithMessage("Số lượng phải từ 1 đến 99.");
    }
}
