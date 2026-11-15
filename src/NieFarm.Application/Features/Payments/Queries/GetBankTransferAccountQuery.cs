using MediatR;
using NieFarm.Application.Features.Payments.Dtos;

namespace NieFarm.Application.Features.Payments.Queries;

/// <summary>Null means bank transfer is unavailable. Public, no scoping.</summary>
public record GetBankTransferAccountQuery : IRequest<BankTransferAccountDto?>;
