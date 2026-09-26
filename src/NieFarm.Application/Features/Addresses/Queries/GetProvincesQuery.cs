using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Addresses.Dtos;

namespace NieFarm.Application.Features.Addresses.Queries;

public record GetProvincesQuery : IRequest<Result<IReadOnlyList<ProvinceDto>>>;
