using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Coupons;
using NieFarm.Application.Features.Coupons;
using NieFarm.Application.Features.Coupons.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Coupons.Commands;

/// <summary>
/// Creates when <paramref name="Id"/> is null, otherwise updates that coupon. Dates are UTC (the admin page
/// converts from Vietnam time). Code and Type cannot change once the coupon has been used.
/// </summary>
public record SaveCouponCommand(
    int? Id,
    string Code,
    string? Description,
    CouponType Type,
    decimal Value,
    decimal? MaxDiscountAmount,
    decimal MinOrderSubtotal,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    int? UsageLimit,
    int? PerCustomerLimit,
    bool IsActive) : IRequest<Result<int>>;

public class SaveCouponCommandValidator : AbstractValidator<SaveCouponCommand>
{
    public SaveCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Must(c => CouponCodeFormat.TryNormalise(c, out _))
            .WithMessage("Code must be 4-30 characters: letters A-Z, digits, hyphen or underscore, starting with a letter or digit.");

        RuleFor(x => x.Description)
            .MaximumLength(Coupon.DescriptionMaxLength).WithMessage("Description is too long (max 200 characters).");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Choose a discount type.");

        RuleFor(x => x.Value)
            .Must((cmd, value) => Coupon.IsValidValue(cmd.Type, value))
            .WithMessage(cmd => cmd.Type switch
            {
                CouponType.Percentage => "Percentage must be a whole number from 1 to 100.",
                CouponType.FixedAmount => "Amount must be a whole amount of at least 1,000 (up to 100,000,000).",
                CouponType.FreeShipping => "A free-shipping coupon has no value.",
                _ => "Value is not valid."
            });

        RuleFor(x => x.MaxDiscountAmount)
            .Must(v => v is null || (v >= 1m && v <= Coupon.MaxMoney && v == decimal.Truncate(v.Value)))
            .WithMessage("Maximum discount must be a whole amount above 0.")
            .Must((cmd, v) => v is null || cmd.Type == CouponType.Percentage)
            .WithMessage("A maximum discount only applies to a percentage coupon.");

        RuleFor(x => x.MinOrderSubtotal)
            .Must(v => v >= 0m && v <= Coupon.MaxMoney && v == decimal.Truncate(v))
            .WithMessage("Minimum order must be a whole amount of 0 or more.");

        RuleFor(x => x.EndsAtUtc)
            .Must((cmd, end) => end is null || cmd.StartsAtUtc is null || end > cmd.StartsAtUtc)
            .WithMessage("The end of the validity window must be after its start.");

        RuleFor(x => x.UsageLimit)
            .Must(v => v is null || (v >= 1 && v <= Coupon.MaxUses))
            .WithMessage("Usage limit must be at least 1 (leave empty for unlimited).");

        RuleFor(x => x.PerCustomerLimit)
            .Must(v => v is null || (v >= 1 && v <= Coupon.MaxUses))
            .WithMessage("Per-customer limit must be at least 1 (leave empty for unlimited).");
    }
}

public class SaveCouponCommandHandler(
    IRepositoryBase<Coupon> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<SaveCouponCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveCouponCommand request, CancellationToken cancellationToken)
    {
        // Decide against the database, not a stale tracked row: the row version must be current.
        contextRefresher.DiscardTrackedEntities();

        CouponCodeFormat.TryNormalise(request.Code, out var code);
        var startsAt = AsUtc(request.StartsAtUtc);
        var endsAt = AsUtc(request.EndsAtUtc);

        Coupon? existing = null;
        if (request.Id is { } id)
        {
            existing = await repository.FirstOrDefaultAsync(new CouponByIdSpec(id, tracked: true), cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            if (existing.TimesRedeemed > 0 && (existing.Code != code || existing.Type != request.Type))
                return Result<int>.Error("A coupon that has been used cannot change its code or type. Disable it and create a new one instead.");
        }

        // Friendly error first; the unique index is the backstop.
        var sameCode = await repository.FirstOrDefaultAsync(new CouponByCodeSpec(code, tracked: true), cancellationToken);
        if (sameCode is not null && sameCode.Id != existing?.Id)
            return Result<int>.Error($"A coupon with the code {code} already exists.");

        try
        {
            if (existing is null)
            {
                var coupon = Coupon.Create(
                    code, request.Description, request.Type, request.Value, request.MaxDiscountAmount,
                    request.MinOrderSubtotal, startsAt, endsAt, request.UsageLimit, request.PerCustomerLimit,
                    request.IsActive);
                await repository.AddAsync(coupon, cancellationToken);
                return Result<int>.Success(coupon.Id);
            }

            existing.Update(
                code, request.Description, request.Type, request.Value, request.MaxDiscountAmount,
                request.MinOrderSubtotal, startsAt, endsAt, request.UsageLimit, request.PerCustomerLimit,
                request.IsActive);
            await repository.UpdateAsync(existing, cancellationToken);
            return Result<int>.Success(existing.Id);
        }
        catch (ConcurrencyConflictException)
        {
            return Result<int>.Conflict(CouponAdminMessages.Conflict);
        }
    }

    private static DateTime? AsUtc(DateTime? value) =>
        value is null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
}
