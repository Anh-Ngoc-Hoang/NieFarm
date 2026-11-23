using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.AddressBook.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Commands;

/// <summary>Two steps, exactly like SetCurrentCropCommandHandler: clear the other defaults, then flag this one.</summary>
public class SetDefaultMyAddressCommandHandler(
    IRepositoryBase<CustomerAddress> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser)
    : IRequestHandler<SetDefaultMyAddressCommand, Result>
{
    public async Task<Result> Handle(SetDefaultMyAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        refresher.DiscardTrackedEntities();
        var book = await repository.ListAsync(new MyAddressesSpec(userId), cancellationToken);

        var target = book.FirstOrDefault(a => a.Id == request.Id);
        if (target is null)
            return Result.NotFound(AddressBookMessages.NotFound);   // also covers "not yours"

        var toClear = book.Where(a => a.IsDefault && !ReferenceEquals(a, target)).ToList();
        if (toClear.Count > 0)
        {
            foreach (var other in toClear)
                other.ClearDefault();

            await repository.UpdateRangeAsync(toClear, cancellationToken);
        }

        // No-op success when the target is already the sole default.
        if (!target.IsDefault)
        {
            target.MarkDefault();
            await repository.UpdateAsync(target, cancellationToken);
        }

        return Result.Success();
    }
}
