using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.AddressBook.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Commands;

public class DeleteMyAddressCommandHandler(
    IRepositoryBase<CustomerAddress> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteMyAddressCommand, Result>
{
    public async Task<Result> Handle(DeleteMyAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        refresher.DiscardTrackedEntities();
        var book = await repository.ListAsync(new MyAddressesSpec(userId), cancellationToken);

        var target = book.FirstOrDefault(a => a.Id == request.Id);
        if (target is null)
            return Result.NotFound(AddressBookMessages.NotFound);   // also covers "not yours"

        var wasDefault = target.IsDefault;

        // Deleting the default first leaves the book with no default, so the unique index is never
        // asked to hold two flags; the successor is flagged in a second SaveChanges.
        await repository.DeleteAsync(target, cancellationToken);

        if (wasDefault)
        {
            var successor = book
                .Where(a => !ReferenceEquals(a, target))
                .OrderByDescending(a => a.UpdatedAt)
                .ThenByDescending(a => a.Id)
                .FirstOrDefault();

            if (successor is not null)
            {
                successor.MarkDefault();
                await repository.UpdateAsync(successor, cancellationToken);
            }
        }

        return Result.Success();
    }
}
