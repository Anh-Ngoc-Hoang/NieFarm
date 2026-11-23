using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Account.Profile.Dtos;

namespace NieFarm.Application.Features.Account.Profile.Queries;

/// <summary>The signed-in customer's own profile (user id comes from ICurrentUser, never the caller).</summary>
public record GetMyProfileQuery : IRequest<Result<MyProfileDto>>;
