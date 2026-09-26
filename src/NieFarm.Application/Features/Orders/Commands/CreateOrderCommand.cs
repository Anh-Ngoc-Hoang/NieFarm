using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Orders.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Commands;

/// <summary>
/// Places a real order from the public checkout. Still carries no money and no line data:
/// quantities come from the shopper's persisted Cart, prices/name/image are re-derived from the
/// Products table (CartAssembler), and Subtotal/Total are computed by the Order aggregate. The
/// customer id is resolved from ICurrentUser, never submitted. AnonymousId is the one addition
/// (D2) — the opaque nf_cart cookie token identifying which guest cart to read and then delete,
/// because a Blazor circuit has no HttpContext from which to read the cookie itself.
/// </summary>
public record CreateOrderCommand(
    string CustomerName,
    string Phone,
    string? Email,
    string Street,
    string Ward,
    string Province,
    string? Note,
    PaymentMethod PaymentMethod,
    string? AnonymousId) : IRequest<Result<OrderPlacedDto>>;
