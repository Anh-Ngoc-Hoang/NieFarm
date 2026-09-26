using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public record MarkOrderReadCommand(int Id) : IRequest<Result>;
