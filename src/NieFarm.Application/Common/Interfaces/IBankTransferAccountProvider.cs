using NieFarm.Application.Features.Payments;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Supplies the beneficiary bank account for bank-transfer orders. Implemented in Infrastructure over
/// configuration. A null result means bank transfer is unavailable (disabled or misconfigured).
/// </summary>
public interface IBankTransferAccountProvider
{
    BankTransferAccount? GetAccount();
}
