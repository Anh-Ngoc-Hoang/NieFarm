using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Payments.Dtos;

namespace NieFarm.Application.Features.Payments.Queries;

public class GetBankTransferAccountQueryHandler(IBankTransferAccountProvider provider)
    : IRequestHandler<GetBankTransferAccountQuery, BankTransferAccountDto?>
{
    public Task<BankTransferAccountDto?> Handle(
        GetBankTransferAccountQuery request, CancellationToken cancellationToken)
    {
        var account = provider.GetAccount();
        BankTransferAccountDto? dto = account is null
            ? null
            : new BankTransferAccountDto(account.BankName, account.AccountNumber, account.AccountName, account.Branch);
        return Task.FromResult(dto);
    }
}
