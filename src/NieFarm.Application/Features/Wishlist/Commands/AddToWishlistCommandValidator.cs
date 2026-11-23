using FluentValidation;

namespace NieFarm.Application.Features.Wishlist.Commands;

public class AddToWishlistCommandValidator : AbstractValidator<AddToWishlistCommand>
{
    public AddToWishlistCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage(WishlistMessages.InvalidProduct);
    }
}
