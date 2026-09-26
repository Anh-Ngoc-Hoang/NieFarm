using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts.Commands;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Không tìm thấy sản phẩm.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, Cart.MaxQuantityPerItem)
            .WithMessage("Số lượng phải từ 1 đến 99.");

        RuleFor(x => x.SelectedOptionValues)
            .NotNull().WithMessage("Vui lòng chọn phiên bản sản phẩm hợp lệ.")
            .Must(v => v.Count <= 10).WithMessage("Vui lòng chọn phiên bản sản phẩm hợp lệ.");

        RuleForEach(x => x.SelectedOptionValues)
            .NotEmpty().WithMessage("Vui lòng chọn phiên bản sản phẩm hợp lệ.")
            .MaximumLength(100).WithMessage("Vui lòng chọn phiên bản sản phẩm hợp lệ.");
    }
}
