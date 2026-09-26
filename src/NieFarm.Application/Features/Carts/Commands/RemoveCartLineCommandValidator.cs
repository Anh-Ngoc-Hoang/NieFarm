using FluentValidation;

namespace NieFarm.Application.Features.Carts.Commands;

public class RemoveCartLineCommandValidator : AbstractValidator<RemoveCartLineCommand>
{
    public RemoveCartLineCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Sản phẩm không có trong giỏ hàng.");

        RuleFor(x => x.VariantKey)
            .MaximumLength(CartLineKey.MaxLength);
    }
}
