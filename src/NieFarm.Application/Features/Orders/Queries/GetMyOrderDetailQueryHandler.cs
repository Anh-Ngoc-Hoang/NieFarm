using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Queries;

public class GetMyOrderDetailQueryHandler(
    IReadRepositoryBase<Order> repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyOrderDetailQuery, Result<MyOrderDetailDto>>
{
    public async Task<Result<MyOrderDetailDto>> Handle(
        GetMyOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var customerId = await currentUser.GetUserIdAsync(cancellationToken);
        if (customerId is null)
            return Result<MyOrderDetailDto>.Unauthorized();

        var order = await repository.FirstOrDefaultAsync(
            new MyOrderByIdSpec(request.Id, customerId), cancellationToken);
        if (order is null)
            return Result<MyOrderDetailDto>.NotFound();   // also covers "not yours"

        return Result<MyOrderDetailDto>.Success(new MyOrderDetailDto(
            order.Id,
            order.Code,
            order.CustomerName,
            order.Phone,
            order.Email,
            order.ShippingAddress,
            order.Note,
            order.Status,
            order.PaymentMethod,
            order.Subtotal,
            order.ShippingFee,
            order.Discount,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new MyOrderLineDto(
                i.ProductName, i.Variant, i.ImageUrl, i.UnitPrice, i.Quantity, i.LineTotal)).ToList()));
    }
}
