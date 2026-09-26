using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.ProductCategories.Commands;

public record DeleteProductCategoryCommand(int Id) : IRequest<Result>;
