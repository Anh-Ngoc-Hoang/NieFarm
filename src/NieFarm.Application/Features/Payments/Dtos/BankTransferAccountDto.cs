namespace NieFarm.Application.Features.Payments.Dtos;

public record BankTransferAccountDto(string BankName, string AccountNumber, string AccountName, string? Branch);
