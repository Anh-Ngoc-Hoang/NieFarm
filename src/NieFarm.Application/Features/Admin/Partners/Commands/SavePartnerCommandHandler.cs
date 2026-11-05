using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Partners.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Partners.Commands;

public class SavePartnerCommandHandler(
    IRepositoryBase<Partner> repository,
    IImageStorage imageStorage)
    : IRequestHandler<SavePartnerCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SavePartnerCommand request, CancellationToken cancellationToken)
    {
        if (request.Id is { } id)
        {
            var existing = await repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            var previousLogoUrl = existing.LogoUrl;

            existing.Update(
                request.Name,
                request.LogoUrl ?? string.Empty,
                request.WebsiteUrl ?? string.Empty,
                request.SortOrder,
                request.IsVisible);

            await repository.UpdateAsync(existing, cancellationToken);

            if (PartnerImageLimits.IsManagedImage(previousLogoUrl)
                && !string.Equals(previousLogoUrl, existing.LogoUrl, StringComparison.OrdinalIgnoreCase))
            {
                await imageStorage.DeleteManyAsync([previousLogoUrl], cancellationToken);
            }

            return Result<int>.Success(existing.Id);
        }

        var partner = Partner.Create(
            request.Name,
            request.LogoUrl ?? string.Empty,
            request.WebsiteUrl ?? string.Empty,
            request.SortOrder,
            request.IsVisible);

        await repository.AddAsync(partner, cancellationToken);
        return Result<int>.Success(partner.Id);
    }
}
