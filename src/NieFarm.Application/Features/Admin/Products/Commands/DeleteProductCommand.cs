using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<Result>;
