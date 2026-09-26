using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Activities.Commands;

public record DeleteActivityCommand(int Id) : IRequest<Result>;
