using FluentValidation;

namespace NieFarm.Application.Features.Orders.Queries;

public class GetMyOrderDetailQueryValidator : AbstractValidator<GetMyOrderDetailQuery>
{
    public GetMyOrderDetailQueryValidator()
    {
        RuleFor(q => q.Id).GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");
    }
}
