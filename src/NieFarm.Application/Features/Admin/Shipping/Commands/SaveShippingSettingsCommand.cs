using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Shipping.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Shipping.Commands;

/// <summary>Saves the base fee and the nationwide free-shipping threshold (null = never free). Creates the row if it is missing.</summary>
public record SaveShippingSettingsCommand(decimal BaseFee, decimal? FreeShippingThreshold) : IRequest<Result>;

public class SaveShippingSettingsCommandValidator : AbstractValidator<SaveShippingSettingsCommand>
{
    public SaveShippingSettingsCommandValidator()
    {
        RuleFor(x => x.BaseFee)
            .InclusiveBetween(0m, ShippingSettings.MaxFee).WithMessage("Base fee must be between 0 and 1,000,000.")
            .Must(v => v == decimal.Truncate(v)).WithMessage("Base fee must be a whole amount (no decimals).");

        RuleFor(x => x.FreeShippingThreshold)
            .Must(v => v is null || v > 0m).WithMessage("The free-shipping threshold must be above 0, or empty for never free.")
            .Must(v => v is null || v == decimal.Truncate(v.Value)).WithMessage("The free-shipping threshold must be a whole amount (no decimals).")
            .Must(v => v is null || v <= 1_000_000_000m).WithMessage("The free-shipping threshold is too large.");
    }
}

public class SaveShippingSettingsCommandHandler(
    IRepositoryBase<ShippingSettings> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<SaveShippingSettingsCommand, Result>
{
    public async Task<Result> Handle(SaveShippingSettingsCommand request, CancellationToken cancellationToken)
    {
        // The shared write context lives for the whole circuit; load against the database.
        contextRefresher.DiscardTrackedEntities();

        var settings = await repository.FirstOrDefaultAsync(new ShippingSettingsSingletonSpec(), cancellationToken);
        if (settings is null)
        {
            await repository.AddAsync(
                ShippingSettings.Create(request.BaseFee, request.FreeShippingThreshold), cancellationToken);
            return Result.Success();
        }

        // Two admins editing at once is last-write-wins; orders snapshot their own fee.
        settings.Update(request.BaseFee, request.FreeShippingThreshold);
        await repository.UpdateAsync(settings, cancellationToken);
        return Result.Success();
    }
}
