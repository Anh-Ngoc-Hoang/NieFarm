using FluentValidation;

namespace NieFarm.Application.Features.Wishlist.Commands;

public class RemoveFromWishlistCommandValidator : AbstractValidator<RemoveFromWishlistCommand>
{
    public RemoveFromWishlistCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage(WishlistMessages.InvalidProduct);
    }
}
