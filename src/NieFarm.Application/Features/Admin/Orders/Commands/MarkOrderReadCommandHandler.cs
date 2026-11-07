using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class MarkOrderReadCommandHandler(
    IRepositoryBase<Order> repository,
    IWriteContextRefresher refresher)
    : IRequestHandler<MarkOrderReadCommand, Result>
{
    public async Task<Result> Handle(MarkOrderReadCommand request, CancellationToken cancellationToken)
    {
        // A customer cancel in another circuit re-flags the order unread; without a fresh load this
        // circuit's cached IsRead = true would swallow it.
        refresher.DiscardTrackedEntities();

        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.NotFound();

        if (order.IsRead)
            return Result.Success();

        order.MarkAsRead();
        try
        {
            await repository.UpdateAsync(order, cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // The order changed between load and save (e.g. a customer cancel, which re-flags it
            // unread). Leaving it unread is the correct outcome; the page ignores this result.
        }

        return Result.Success();
    }
}
