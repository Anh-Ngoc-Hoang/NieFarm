using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Application.Features.Payments;
using NieFarm.Domain.Enums;
using NieFarm.Application.Features.Orders.Specifications;
using DomainCart = NieFarm.Domain.Entities.Cart;
using Order = NieFarm.Domain.Entities.Order;

namespace NieFarm.Application.Features.Orders.Commands;

public class CreateOrderCommandHandler(
    IRepositoryBase<Order> repository,
    IRepositoryBase<DomainCart> cartRepository,
    CartAssembler assembler,
    IWriteContextRefresher contextRefresher,
    ICurrentUser currentUser,
    IBankTransferAccountProvider bankAccountProvider,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, Result<OrderPlacedDto>>
{
    public async Task<Result<OrderPlacedDto>> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // The shared write context lives for the whole circuit, so an earlier-tracked cart could be
        // stale; decide against the database (same reason as the R-07 order handlers).
        contextRefresher.DiscardTrackedEntities();

        // Null is a normal, supported outcome — guest checkout.
        var customerId = await currentUser.GetUserIdAsync(cancellationToken);

        // Write repo, because this handler will delete the row once the order is placed (D13).
        var cartEntity = await cartRepository.FirstOrDefaultAsync(
            new CartByOwnerSpec(customerId, request.AnonymousId), cancellationToken);

        var cart = await assembler.BuildAsync(cartEntity, new CartPricingContext(request.Province), cancellationToken);
        if (cart.IsEmpty)
            return Result<OrderPlacedDto>.Error("Giỏ hàng đang trống.");

        // data-access.md's "re-check availability … at checkout time" landing for real: a line
        // whose product/variant has gone unavailable since it was added must block the order
        // rather than silently being charged at a stale price or dropped.
        if (cart.HasUnavailableLines)
            return Result<OrderPlacedDto>.Error("Một số sản phẩm trong giỏ hàng đã hết hàng. Vui lòng cập nhật giỏ hàng.");

        // Intent check: the shopper confirmed ExpectedTotal; if a price, fee, zone, threshold or line
        // availability changed since, make them look again. Never used as a price.
        if (request.ExpectedTotal != cart.Total)
            return Result<OrderPlacedDto>.Conflict(PricingMessages.TotalChanged);

        if (request.PaymentMethod == PaymentMethod.BankTransfer && cart.Total <= 0m)
            return Result<OrderPlacedDto>.Error(PricingMessages.ZeroTotalBankTransfer);

        // The UI is not trusted, and the account can be switched off between page load and submit.
        // Checked before a code is generated so a rejected order consumes nothing.
        var bankAccount = bankAccountProvider.GetAccount();
        if (request.PaymentMethod == PaymentMethod.BankTransfer && bankAccount is null)
            return Result<OrderPlacedDto>.Error(
                "Chuyển khoản ngân hàng tạm thời chưa khả dụng. Vui lòng chọn phương thức thanh toán khác.");

        var address = string.Join(", ", new[] { request.Street, request.Ward, request.Province }
            .Select(p => p.Trim())
            .Where(p => p.Length > 0));

        // Generate a unique code (loop, max 5 attempts). Both the read and the write go
        // through IRepositoryBase<Order> (the shared scoped context), so there is no
        // cross-context tracking hazard. The unique index is the hard guarantee.
        string code;
        var attempt = 0;
        do
        {
            code = OrderCode.Generate();
            attempt++;
        }
        while (await repository.AnyAsync(new OrderByCodeSpec(code), cancellationToken) && attempt < 5);

        var order = Order.Create(
            code, customerId, request.CustomerName, request.Phone, request.Email,
            address, request.Note, request.PaymentMethod,
            cart.ShippingFee, request.Province, cart.ShippingZoneName);

        // No stock decrement: the catalog has no stock quantity at all, only
        // ProductVariant.IsAvailable, so there is nothing to decrement. Availability was
        // re-checked above and prices/name/image come from the Products table via
        // CartAssembler, which is what keeps security.md satisfied.
        foreach (var line in cart.Lines)
            order.AddItem(line.ProductId, line.Name, Truncate(line.Variant, 150), line.ImageUrl, line.UnitPrice, line.Quantity);

        await repository.AddAsync(order, cancellationToken);

        // Not atomic with the write above — Repository<T> saves per operation and NieFarm has
        // no unit-of-work seam. This ordering (create the order first, delete the cart second)
        // fails safe: the worst outcome is an order that exists with a cart that was not
        // cleaned up ("my cart is still full"), never an order that silently vanished. See
        // docs/plans/cart-quantity-removal.md §10.
        if (cartEntity is not null)
        {
            try
            {
                await cartRepository.DeleteAsync(cartEntity, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The order is already saved, so a failed cart clean-up (a concurrency conflict because
                // the row is gone or changed, a timeout, ...) must never turn into a failure. The
                // worst outcome is a cart that is still full.
                logger.LogWarning(ex, "Order {Code} was placed but its cart could not be deleted", order.Code);
            }
        }

        return Result<OrderPlacedDto>.Success(new OrderPlacedDto(
            order.Id, order.Code, order.PaymentMethod, order.Total,
            BankTransferInstructions.Build(order, bankAccount)));
    }

    /// <summary>Guards against a multi-axis variant display overflowing OrderItem.Variant's nvarchar(150).</summary>
    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
