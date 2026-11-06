using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Orders.Dtos;

namespace NieFarm.Application.Features.Orders.Queries;

public record LookupOrderQuery(string Code, string Phone) : IRequest<Result<OrderLookupDto>>;
