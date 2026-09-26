using MediatR;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public record GetUnreadOrderCountQuery : IRequest<int>;
