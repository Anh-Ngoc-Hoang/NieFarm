using Ardalis.Specification;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Products;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts;

/// <summary>
/// The single path used to build a CartDto for both display (GetCartQuery) and charge
/// (CreateOrderCommandHandler), plus every command that returns a rebuilt cart. Lines are
/// ordered by CartItem.Id (insertion order) — a stable, server-defined display order — so "what
/// you saw is what you are charged" is structural, not conventional. The price book is the
/// Products table, re-read on every call: name, image, variant label, price and availability are
/// never trusted from the stored CartItem row (security.md). A line whose product was deleted or
/// hidden is silently dropped (D8) — there is no data left to render it with, which is also what
/// keeps a hand-inserted CartItems row from ever being priced. A line whose variant is now
/// unavailable, or whose label combination no longer resolves to any variant, is kept and
/// flagged IsAvailable = false (D9) and excluded from Subtotal/ItemCount/ShippingFee, so the
/// displayed total is never a number the shopper cannot actually pay.
///
/// Public (not internal, despite the rest of this feature's helpers) because it is injected as
/// a constructor parameter into public MediatR handlers (GetCartQueryHandler,
/// AddToCartCommandHandler, SetCartLineQuantityCommandHandler, RemoveCartLineCommandHandler,
/// CreateOrderCommandHandler) — an internal type there would fail to compile (CS0053).
/// </summary>
public sealed class CartAssembler(IReadRepositoryBase<Product> products)
{
    public async Task<CartDto> BuildAsync(DomainCart? cart, CancellationToken cancellationToken)
    {
        if (cart is null || cart.Items.Count == 0)
            return CartDto.Empty;

        var ids = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var rows = await products.ListAsync(new ProductsByIdsForCartSpec(ids), cancellationToken);
        var productsById = rows.ToDictionary(p => p.Id);

        var lines = new List<CartLineDto>();
        foreach (var item in cart.Items.OrderBy(i => i.Id))
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
                continue; // deleted or hidden — no data left to render (D8)

            var labels = CartLineKey.Split(item.VariantKey);
            var variant = ProductVariantResolver.FindVariant(product, labels);

            var unitPrice = variant?.Price ?? product.Price;
            var isAvailable = product.Options.Count == 0 || variant is { IsAvailable: true };

            var imageAlt = string.IsNullOrWhiteSpace(product.ImageAlt) ? product.Name : product.ImageAlt;
            var quantity = Math.Clamp(item.Quantity, 1, DomainCart.MaxQuantityPerItem);

            lines.Add(new CartLineDto(
                product.Id,
                item.VariantKey,
                product.Slug,
                product.Name,
                CartLineKey.ToDisplay(labels),
                product.ImageUrl,
                imageAlt,
                unitPrice,
                quantity,
                unitPrice * quantity,
                isAvailable));
        }

        var availableLines = lines.Where(l => l.IsAvailable).ToList();
        var subtotal = availableLines.Sum(l => l.LineTotal);
        var itemCount = availableLines.Sum(l => l.Quantity);
        var shippingFee = availableLines.Count == 0 ? 0m : CartPricing.ShippingFee;
        var discount = CartPricing.Discount;

        return new CartDto(lines, itemCount, subtotal, shippingFee, discount,
            subtotal + shippingFee - discount);
    }
}
