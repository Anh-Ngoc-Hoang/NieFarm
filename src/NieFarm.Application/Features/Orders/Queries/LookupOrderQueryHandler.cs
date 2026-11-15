using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Application.Features.Payments;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Queries;

public class LookupOrderQueryHandler(
    IReadRepositoryBase<Order> repository,
    IOrderLookupThrottle throttle,
    IBankTransferAccountProvider bankAccountProvider)
    : IRequestHandler<LookupOrderQuery, Result<OrderLookupDto>>
{
    public async Task<Result<OrderLookupDto>> Handle(LookupOrderQuery request, CancellationToken cancellationToken)
    {
        if (!OrderCode.TryNormalise(request.Code, out var code))
            return Result<OrderLookupDto>.NotFound();
        var phone = PhoneNumberNormalizer.Normalise(request.Phone);

        if (throttle.IsLockedOut(code, phone))
            return Result<OrderLookupDto>.Error(OrderLookupMessages.LockedOut);

        // Unlike MyOrderByIdSpec the phone check is not in the WHERE: stored phones are free-form
        // text, so it is compared in normalised form here. Unknown code and wrong phone share one
        // branch so the two are indistinguishable to the caller.
        var order = await repository.FirstOrDefaultAsync(new OrderByCodeWithItemsSpec(code), cancellationToken);
        if (order is null || !PhoneNumberNormalizer.Matches(order.Phone, phone))
        {
            throttle.RecordFailure(code, phone);
            return Result<OrderLookupDto>.NotFound();
        }

        return Result<OrderLookupDto>.Success(new OrderLookupDto(
            order.Code,
            order.CreatedAt,
            order.Status,
            order.PaymentMethod,
            OrderLookupMasking.MaskName(order.CustomerName),
            OrderLookupMasking.MaskPhone(PhoneNumberNormalizer.Normalise(order.Phone)),
            OrderLookupMasking.DeliveryArea(order.ShippingAddress),
            order.Subtotal,
            order.ShippingFee,
            order.Discount,
            order.Total,
            order.Items.Select(i => new OrderLookupLineDto(
                i.ProductName, i.Variant, i.ImageUrl, i.UnitPrice, i.Quantity, i.LineTotal)).ToList(),
            order.IsCancellableByCustomer && order.CustomerId is null,
            order.IsCancellableByCustomer && order.CustomerId is not null,
            OrderTimeline.From(order),
            order.PaymentStatus,
            order.PaidAt,
            BankTransferInstructions.Build(order, bankAccountProvider.GetAccount()),
            order.ShippingZoneName,
            order.CouponCode));
    }
}
