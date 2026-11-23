using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Application.Features.Products;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Orders.Commands;

public class ReorderCommandHandler(
    IReadRepositoryBase<Order> orderRepository,
    IReadRepositoryBase<Product> productRepository,
    IRepositoryBase<DomainCart> cartRepository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser)
    : IRequestHandler<ReorderCommand, Result<ReorderResultDto>>
{
    public async Task<Result<ReorderResultDto>> Handle(ReorderCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
            return Result<ReorderResultDto>.Unauthorized();

        // Read repo, entities used for values only and then dropped; never handed to the write repo
        // (data-access.md). Ownership is part of the WHERE, so "not yours" and "missing" are one NotFound.
        var order = await orderRepository.FirstOrDefaultAsync(
            new MyOrderByIdSpec(request.OrderId, userId), cancellationToken);
        if (order is null)
            return Result<ReorderResultDto>.NotFound();

        var productIds = order.Items
            .Where(i => i.ProductId.HasValue)
            .Select(i => i.ProductId!.Value)
            .Distinct()
            .ToList();

        // Visible products only: a deleted or hidden one is simply absent from the dictionary.
        var products = productIds.Count == 0
            ? new Dictionary<int, Product>()
            : (await productRepository.ListAsync(new ProductsByIdsForCartSpec(productIds), cancellationToken))
                .ToDictionary(p => p.Id);

        // Decide every line against the CURRENT catalog first. Nothing here reads a price: the cart prices itself.
        var candidates = new List<(int ProductId, string Key, int Quantity, string DisplayName)>();
        var skipped = new List<ReorderSkippedLineDto>();

        foreach (var item in order.Items)
        {
            var displayName = string.IsNullOrWhiteSpace(item.Variant)
                ? item.ProductName
                : $"{item.ProductName} ({item.Variant})";

            if (item.ProductId is not { } productId || !products.TryGetValue(productId, out var product))
            {
                skipped.Add(new ReorderSkippedLineDto(displayName, ReorderSkipReason.NoLongerSold));
                continue;
            }

            var resolved = OrderLineVariantKey.Resolve(item, product);
            if (resolved is null)
            {
                skipped.Add(new ReorderSkippedLineDto(displayName, ReorderSkipReason.OptionsChanged));
                continue;
            }

            var variant = product.Options.Count > 0
                ? ProductVariantResolver.FindVariant(product, resolved.Value.Labels)
                : null;

            // A label combination that is no longer a variant at all is an options change, not a stock problem.
            if (product.Options.Count > 0 && variant is null)
            {
                skipped.Add(new ReorderSkippedLineDto(displayName, ReorderSkipReason.OptionsChanged));
                continue;
            }

            if (!ProductAvailability.IsVariantPurchasable(product, variant))
            {
                skipped.Add(new ReorderSkippedLineDto(displayName, ReorderSkipReason.Unavailable));
                continue;
            }

            candidates.Add((product.Id, resolved.Value.Key, item.Quantity, displayName));
        }

        // From here on only the write repo: drop whatever an earlier command left tracked, then load the cart fresh.
        refresher.DiscardTrackedEntities();

        var cart = await cartRepository.FirstOrDefaultAsync(
            new CartByOwnerSpec(userId, null), cancellationToken);
        var isNewCart = cart is null;
        cart ??= DomainCart.ForUser(userId);

        var added = 0;
        foreach (var line in candidates)
        {
            if (cart.IsFullFor(line.ProductId, line.Key))
            {
                skipped.Add(new ReorderSkippedLineDto(line.DisplayName, ReorderSkipReason.CartFull));
                continue;
            }

            // Original quantity, clamped to 99 by the aggregate.
            cart.AddOrIncrement(line.ProductId, line.Key, line.Quantity);
            added++;
        }

        if (added > 0)
        {
            try
            {
                if (isNewCart)
                    await cartRepository.AddAsync(cart, cancellationToken);
                else
                    await cartRepository.UpdateAsync(cart, cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                // The cart changed (or was purged) between load and save; nothing was written.
                return Result<ReorderResultDto>.Conflict(OrderMessages.ReorderConflict);
            }
        }

        return Result<ReorderResultDto>.Success(new ReorderResultDto(added, cart.TotalQuantity, skipped));
    }
}
