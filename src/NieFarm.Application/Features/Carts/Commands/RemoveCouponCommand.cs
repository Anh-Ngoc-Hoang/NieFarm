using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// Forgets the cart's coupon code. Idempotent: no cart, or no code, is a success returning the rebuilt cart.
/// </summary>
public record RemoveCouponCommand(string? AnonymousId, string? Province = null) : IRequest<Result<CartDto>>;

public class RemoveCouponCommandValidator : AbstractValidator<RemoveCouponCommand>
{
    public RemoveCouponCommandValidator()
    {
        RuleFor(x => x.Province).MaximumLength(120);
    }
}

public class RemoveCouponCommandHandler(
    IRepositoryBase<DomainCart> cartRepository,
    CartAssembler assembler,
    ICurrentUser currentUser,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<RemoveCouponCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(request.AnonymousId))
            return Result<CartDto>.Error("Không xác định được giỏ hàng.");

        contextRefresher.DiscardTrackedEntities();

        var cart = await cartRepository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);
        if (cart is null)
            return Result<CartDto>.Success(CartDto.Empty);

        if (cart.CouponCode is not null)
        {
            cart.RemoveCoupon();
            await cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return Result<CartDto>.Success(
            await assembler.BuildAsync(cart, new CartPricingContext(request.Province, userId), cancellationToken));
    }
}
