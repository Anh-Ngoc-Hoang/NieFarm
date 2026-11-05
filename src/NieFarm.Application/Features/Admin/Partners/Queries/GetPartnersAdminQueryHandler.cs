using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Partners.Dtos;
using NieFarm.Application.Features.Partners.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Partners.Queries;

public class GetPartnersAdminQueryHandler(IReadRepositoryBase<Partner> repository)
    : IRequestHandler<GetPartnersAdminQuery, List<PartnerAdminListDto>>
{
    public async Task<List<PartnerAdminListDto>> Handle(
        GetPartnersAdminQuery request, CancellationToken cancellationToken)
    {
        var partners = await repository.ListAsync(new AllPartnersAdminSpec(), cancellationToken);

        return partners.Select(p => new PartnerAdminListDto(
            p.Id,
            p.Name,
            p.LogoUrl,
            p.WebsiteUrl,
            p.SortOrder,
            p.IsVisible)).ToList();
    }
}
