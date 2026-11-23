using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.AddressBook.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Commands;

/// <summary>
/// The write path follows the R-07 pattern: ICurrentUser for the owner, DiscardTrackedEntities so the
/// decision is made on database state (the write context lives as long as the circuit), the write
/// repository for both load and save. Moving the default is the two-step sequence of
/// SetCurrentCropCommandHandler (D-12): every other default is cleared in its own SaveChanges, then
/// this row is saved with the flag in a second one, never both in one batch.
/// </summary>
public class SaveMyAddressCommandHandler(
    IRepositoryBase<CustomerAddress> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser)
    : IRequestHandler<SaveMyAddressCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveMyAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result<int>.Unauthorized();

        var province = AddressProvinceResolver.Resolve(request.ProvinceCode, request.ProvinceName);
        if (province is null)
            return Result<int>.Error(AddressBookMessages.ProvinceInvalid);

        refresher.DiscardTrackedEntities();
        var book = await repository.ListAsync(new MyAddressesSpec(userId), cancellationToken);

        CustomerAddress? existing = null;
        if (request.Id is { } id)
        {
            // A foreign id is simply not in this user's book.
            existing = book.FirstOrDefault(a => a.Id == id);
            if (existing is null)
                return Result<int>.NotFound(AddressBookMessages.NotFound);
        }
        else if (book.Count >= CustomerAddress.MaxPerUser)
        {
            return Result<int>.Error(AddressBookMessages.LimitReached);
        }

        // The first address is always the default. A book with no default at all (an interrupted
        // two-step move) heals on the next save rather than staying without one.
        var wantDefault = request.MakeDefault
            || existing?.IsDefault == true
            || !book.Any(a => a.IsDefault);

        if (wantDefault)
        {
            var toClear = book.Where(a => a.IsDefault && !ReferenceEquals(a, existing)).ToList();
            if (toClear.Count > 0)
            {
                foreach (var other in toClear)
                    other.ClearDefault();

                // Step 1: the other default(s) lose the flag, in a SaveChanges of their own.
                await repository.UpdateRangeAsync(toClear, cancellationToken);
            }
        }

        // Step 2 (the only step when the default is not moving): the row itself, with its flag.
        CustomerAddress address;
        if (existing is not null)
        {
            address = existing;
            address.Update(request.RecipientName, request.Phone, request.Street,
                province.Value.Code, province.Value.Name, request.WardCode, request.WardName);
            if (wantDefault)
                address.MarkDefault();
            await repository.UpdateAsync(address, cancellationToken);
        }
        else
        {
            address = CustomerAddress.Create(userId, request.RecipientName, request.Phone, request.Street,
                province.Value.Code, province.Value.Name, request.WardCode, request.WardName);
            if (wantDefault)
                address.MarkDefault();
            await repository.AddAsync(address, cancellationToken);
        }

        return Result<int>.Success(address.Id);
    }
}
