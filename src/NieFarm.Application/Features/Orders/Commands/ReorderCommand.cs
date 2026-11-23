using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Orders.Dtos;

namespace NieFarm.Application.Features.Orders.Commands;

/// <summary>
/// "Buy again": puts the lines of one of the signed-in customer's own orders back into their cart. The user id
/// comes from ICurrentUser, and no price crosses this boundary: the cart re-prices every line from the catalog.
/// </summary>
public record ReorderCommand(int OrderId) : IRequest<Result<ReorderResultDto>>;
