using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Orders.Dtos;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public class GetOrderDetailQueryHandler(IReadRepositoryBase<Order> repository)
    : IRequestHandler<GetOrderDetailQuery, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(
        GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.FirstOrDefaultAsync(new OrderByIdSpec(request.Id), cancellationToken);
        if (order is null)
            return null;

        return new OrderDetailDto(
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
            order.Items.Select(i => new OrderLineDto(
                i.ProductName, i.Variant, i.ImageUrl, i.UnitPrice, i.Quantity, i.LineTotal)).ToList(),
            order.AvailableNextStatuses(),
            order.StatusChanges
                .OrderBy(c => c.ChangedAt).ThenBy(c => c.Id)
                .Select(c => new OrderStatusChangeDto(
                    c.FromStatus, c.ToStatus, c.ChangedAt, c.ChangedBy,
                    c.ChangedByUserId != null, c.ChangedByName, c.Note))
                .ToList(),
            order.PaymentStatus,
            order.PaidAt,
            order.PaidAmount,
            Order.AllowedNextStatuses(order.Status).Where(order.IsBlockedByPayment).ToList(),
            order.CanConfirmPayment,
            order.CanRevertPayment,
            order.CanMarkRefunded,
            order.PaymentChanges
                .OrderBy(c => c.ChangedAt).ThenBy(c => c.Id)
                .Select(c => new OrderPaymentChangeDto(
                    c.FromStatus, c.ToStatus, c.ChangedAt, c.ChangedBy,
                    c.ChangedByName, c.Amount, c.Reference, c.Note))
                .ToList(),
            order.ShippingProvince,
            order.ShippingZoneName,
            order.CouponCode);
    }
}
