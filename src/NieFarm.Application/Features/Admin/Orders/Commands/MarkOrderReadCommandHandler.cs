using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class MarkOrderReadCommandHandler(IRepositoryBase<Order> repository)
    : IRequestHandler<MarkOrderReadCommand, Result>
{
    public async Task<Result> Handle(MarkOrderReadCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.NotFound();

        if (order.IsRead)
            return Result.Success();

        order.MarkAsRead();
        await repository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}
