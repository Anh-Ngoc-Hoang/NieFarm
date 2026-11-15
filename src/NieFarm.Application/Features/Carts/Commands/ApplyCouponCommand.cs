using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Application.Features.Coupons;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts.Commands;

/// <summary>
/// Tries a promo code on the shopper's cart and, if it is usable, remembers it on the cart. The user id is
/// resolved server-side; the browser never sends an amount. <paramref name="Province"/> is a pricing input
/// only (it prices the returned cart). Nothing here redeems the coupon: redemption happens when the order is
/// placed (CreateOrderCommandHandler).
/// </summary>
public record ApplyCouponCommand(string Code, string? AnonymousId, string? Province = null)
    : IRequest<Result<CartDto>>;

public class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        // The code format is checked in the handler so that a malformed guess counts as a failure too.
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Vui lòng nhập mã giảm giá.")
            .MaximumLength(100).WithMessage(CouponMessages.Invalid);

        RuleFor(x => x.Province).MaximumLength(120);
    }
}

public class ApplyCouponCommandHandler(
    IRepositoryBase<DomainCart> cartRepository,
    CartAssembler assembler,
    ICurrentUser currentUser,
    ICouponAttemptThrottle throttle,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<ApplyCouponCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var ownerKey = CouponCodeFormat.OwnerKey(userId, request.AnonymousId);
        if (ownerKey is null)
            return Result<CartDto>.Error("Không xác định được giỏ hàng.");

        // Anti-guessing: while locked out every apply is refused, even a valid code.
        if (throttle.IsLockedOut(ownerKey))
            return Result<CartDto>.Error(CouponMessages.LockedOut);

        if (!CouponCodeFormat.TryNormalise(request.Code, out var code))
        {
            throttle.RecordFailure(ownerKey);
            return Result<CartDto>.Error(CouponMessages.Invalid);
        }

        // The write context is circuit-long; decide against the database, not a stale tracked cart.
        contextRefresher.DiscardTrackedEntities();

        var cart = await cartRepository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, request.AnonymousId), cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result<CartDto>.Error(CouponMessages.EmptyCart);

        // Evaluate the typed code WITHOUT touching the tracked cart: a rejected code must leave no
        // pending change behind on the shared context.
        var context = new CartPricingContext(request.Province, userId, CouponCodeOverride: code);
        var dto = await assembler.BuildAsync(cart, context, cancellationToken);

        if (dto.Coupon is null)
            return Result<CartDto>.Error(CouponMessages.EmptyCart);   // nothing payable in the cart

        if (dto.Coupon.Eligibility != Domain.Enums.CouponEligibility.Eligible)
        {
            if (CouponMessages.CountsAsFailure(dto.Coupon.Eligibility))
                throttle.RecordFailure(ownerKey);

            return Result<CartDto>.Error(dto.Coupon.Message ?? CouponMessages.Invalid);
        }

        // Eligible. A code that lowers nothing right now (FreeShipping on an already-free order) is still
        // remembered: the DTO carries the explanation and "Gỡ mã" stays available.
        cart.ApplyCoupon(code);
        await cartRepository.UpdateAsync(cart, cancellationToken);

        return Result<CartDto>.Success(dto);
    }
}
