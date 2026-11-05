using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Partners.Commands;

public record DeletePartnerCommand(int Id) : IRequest<Result>;
