using FluentValidation;

namespace NieFarm.Application.Features.Carts.Commands;

public class PurgeAbandonedCartsCommandValidator : AbstractValidator<PurgeAbandonedCartsCommand>
{
    public PurgeAbandonedCartsCommandValidator()
    {
        RuleFor(x => x.EmptyCartIdleDays).InclusiveBetween(1, 60);

        // null/0 = rule C off; otherwise it must not delete carts a shopper could plausibly reopen,
        // and the upper bound keeps "now - N days" from overflowing.
        RuleFor(x => x.IdleCartDays)
            .Must(d => d is null or 0 ||
                       (d >= CartRetentionPolicy.MinIdleCartDays && d <= CartRetentionPolicy.MaxIdleCartDays))
            .WithMessage($"IdleCartDays must be 0 (off) or between {CartRetentionPolicy.MinIdleCartDays} and {CartRetentionPolicy.MaxIdleCartDays}.");

        // 1000 keeps "Id IN (@p...)" well below SQL Server's 2100 parameter limit.
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 1000);
        RuleFor(x => x.MaxBatches).InclusiveBetween(1, 100);
    }
}
