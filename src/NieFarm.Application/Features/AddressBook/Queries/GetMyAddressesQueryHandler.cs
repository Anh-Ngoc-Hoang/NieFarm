using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.AddressBook.Dtos;
using NieFarm.Application.Features.AddressBook.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Queries;

public class GetMyAddressesQueryHandler(
    IReadRepositoryBase<CustomerAddress> repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyAddressesQuery, Result<IReadOnlyList<CustomerAddressDto>>>
{
    public async Task<Result<IReadOnlyList<CustomerAddressDto>>> Handle(
        GetMyAddressesQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result<IReadOnlyList<CustomerAddressDto>>.Unauthorized();

        var rows = await repository.ListAsync(new AddressesByUserSpec(userId), cancellationToken);
        return Result<IReadOnlyList<CustomerAddressDto>>.Success(rows.Select(AddressBookMapper.ToDto).ToList());
    }
}
