using MediatR;
using NieFarm.Application.Features.Admin.Partners.Dtos;

namespace NieFarm.Application.Features.Admin.Partners.Queries;

public record GetPartnerForEditQuery(int Id) : IRequest<PartnerEditDto?>;
