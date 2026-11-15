using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Shipping;
using NieFarm.Application.Features.Shipping.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Shipping.Commands;

/// <summary>Creates when <paramref name="Id"/> is null, otherwise updates that zone (name, fee, order, active, provinces).</summary>
public record SaveShippingZoneCommand(
    int? Id,
    string Name,
    decimal Fee,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<int> ProvinceCodes) : IRequest<Result<int>>;

public class SaveShippingZoneCommandValidator : AbstractValidator<SaveShippingZoneCommand>
{
    public SaveShippingZoneCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Zone name is required.")
            .MaximumLength(ShippingZone.NameMaxLength).WithMessage("Zone name is too long (max 100 characters).");

        RuleFor(x => x.Fee)
            .InclusiveBetween(0m, ShippingSettings.MaxFee).WithMessage("Fee must be between 0 and 1,000,000.")
            .Must(v => v == decimal.Truncate(v)).WithMessage("Fee must be a whole amount (no decimals).");

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 9999).WithMessage("Order must be between 0 and 9999.");

        RuleFor(x => x.ProvinceCodes)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Select at least one province.")
            .Must(c => c is { Count: > 0 }).WithMessage("Select at least one province.")
            .Must(c => c.Distinct().Count() == c.Count).WithMessage("A province is selected twice.")
            .Must(c => c.All(code => VietnamProvinceCatalog.Find(code) is not null))
            .WithMessage("One of the selected provinces is not in the province list.");
    }
}

public class SaveShippingZoneCommandHandler(
    IRepositoryBase<ShippingZone> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<SaveShippingZoneCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveShippingZoneCommand request, CancellationToken cancellationToken)
    {
        contextRefresher.DiscardTrackedEntities();

        var zones = await repository.ListAsync(new ShippingZonesSpec(activeOnly: false), cancellationToken);
        var name = request.Name.Trim();

        ShippingZone? existing = null;
        if (request.Id is { } id)
        {
            existing = zones.FirstOrDefault(z => z.Id == id);
            if (existing is null)
                return Result<int>.NotFound();
        }

        // Friendly errors first; the unique indexes are the backstop.
        if (zones.Any(z => z.Id != existing?.Id && string.Equals(z.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Result<int>.Error($"A zone named \"{name}\" already exists.");

        foreach (var code in request.ProvinceCodes)
        {
            var owner = zones.FirstOrDefault(z =>
                z.Id != existing?.Id && z.Provinces.Any(p => p.ProvinceCode == code));
            if (owner is not null)
                return Result<int>.Error(
                    $"{VietnamProvinceCatalog.DisplayName(code)} already belongs to zone {owner.Name}.");
        }

        if (existing is null)
        {
            var zone = ShippingZone.Create(name, request.Fee, request.SortOrder, request.IsActive);
            zone.SetProvinces(request.ProvinceCodes);
            await repository.AddAsync(zone, cancellationToken);
            return Result<int>.Success(zone.Id);
        }

        existing.Update(name, request.Fee, request.SortOrder, request.IsActive);
        existing.SetProvinces(request.ProvinceCodes);
        await repository.UpdateAsync(existing, cancellationToken);
        return Result<int>.Success(existing.Id);
    }
}
