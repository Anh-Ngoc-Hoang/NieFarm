using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Addresses.Dtos;

namespace NieFarm.Application.Features.Addresses.Queries;

public record GetWardsQuery(int ProvinceCode) : IRequest<Result<IReadOnlyList<WardDto>>>;
