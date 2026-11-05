using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Partners.Dtos;
using NieFarm.Application.Features.Partners.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Partners.Queries;

public class GetPartnersQueryHandler(IReadRepositoryBase<Partner> repository)
    : IRequestHandler<GetPartnersQuery, List<PartnerDto>>
{
    public async Task<List<PartnerDto>> Handle(
        GetPartnersQuery request, CancellationToken cancellationToken)
    {
        var partners = await repository.ListAsync(new VisiblePartnersSpec(), cancellationToken);

        return partners.Select(p => new PartnerDto(
            p.Id,
            p.Name,
            string.IsNullOrWhiteSpace(p.LogoUrl) ? null : p.LogoUrl,
            string.IsNullOrWhiteSpace(p.WebsiteUrl) ? null : p.WebsiteUrl)).ToList();
    }
}
