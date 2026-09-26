using FluentValidation;

namespace NieFarm.Application.Features.Addresses.Queries;

public class GetWardsQueryValidator : AbstractValidator<GetWardsQuery>
{
    public GetWardsQueryValidator() =>
        RuleFor(q => q.ProvinceCode).GreaterThan(0).WithMessage("Mã tỉnh/thành không hợp lệ.");
}
