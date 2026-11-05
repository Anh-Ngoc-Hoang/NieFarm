using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Partners.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Partners.Queries;

public class GetPartnerForEditQueryHandler(IReadRepositoryBase<Partner> repository)
    : IRequestHandler<GetPartnerForEditQuery, PartnerEditDto?>
{
    public async Task<PartnerEditDto?> Handle(
        GetPartnerForEditQuery request, CancellationToken cancellationToken)
    {
        var partner = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
            return null;

        return new PartnerEditDto(
            partner.Id,
            partner.Name,
            partner.LogoUrl,
            partner.WebsiteUrl,
            partner.SortOrder,
            partner.IsVisible);
    }
}
