using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Account.Commands;

public class RegisterCustomerCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCustomerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterCustomerAsync(
            request.Email, request.FullName, request.Password, request.PhoneNumber, cancellationToken);

        if (result.Status == ResultStatus.Conflict)
            return Result<string>.Error("Email này đã được đăng ký. Vui lòng đăng nhập.");

        return result.IsSuccess
            ? result
            : Result<string>.Error("Không thể tạo tài khoản. Vui lòng kiểm tra lại thông tin.");
    }
}
