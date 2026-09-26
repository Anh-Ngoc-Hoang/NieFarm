using MediatR;
using NieFarm.Application.Features.Admin.Accounts.Dtos;

namespace NieFarm.Application.Features.Admin.Accounts.Queries;

public record GetAccountsQuery : IRequest<List<AccountDto>>;
