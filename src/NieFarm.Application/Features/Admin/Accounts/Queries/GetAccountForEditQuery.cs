using MediatR;
using NieFarm.Application.Features.Admin.Accounts.Dtos;

namespace NieFarm.Application.Features.Admin.Accounts.Queries;

public record GetAccountForEditQuery(string Id) : IRequest<AccountDto?>;
