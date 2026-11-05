using MediatR;
using NieFarm.Application.Features.Partners.Dtos;

namespace NieFarm.Application.Features.Partners.Queries;

/// <summary>
/// A bare list, not Result&lt;List&lt;PartnerDto&gt;&gt;, because it has no failure mode
/// (mirrors GetTestimonialsQuery). Public content — no scoping, no ICurrentUser.
/// </summary>
public record GetPartnersQuery : IRequest<List<PartnerDto>>;
