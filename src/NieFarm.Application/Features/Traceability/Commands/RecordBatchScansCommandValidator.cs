using FluentValidation;

namespace NieFarm.Application.Features.Traceability.Commands;

public class RecordBatchScansCommandValidator : AbstractValidator<RecordBatchScansCommand>
{
    public const int MaxTallies = 500;

    public RecordBatchScansCommandValidator()
    {
        RuleFor(x => x.Tallies)
            .NotEmpty().WithMessage("At least one tally is required.")
            .Must(t => t.Count <= MaxTallies).WithMessage($"At most {MaxTallies} tallies per call.");

        RuleForEach(x => x.Tallies).ChildRules(tally =>
        {
            tally.RuleFor(t => t.BatchId).GreaterThan(0);
            tally.RuleFor(t => t.Count).GreaterThan(0);
            tally.RuleFor(t => t.LastAtUtc).GreaterThanOrEqualTo(t => t.FirstAtUtc);
        });

        RuleFor(x => x.DailyAlertThreshold).InclusiveBetween(0, 100_000);
        RuleFor(x => x.PackCountAlertMultiplier).InclusiveBetween(0, 100);
    }
}
