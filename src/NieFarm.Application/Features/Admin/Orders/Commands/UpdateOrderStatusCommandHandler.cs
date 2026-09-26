using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public class UpdateOrderStatusCommandHandler(IRepositoryBase<Order> repository)
    : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.NotFound();

        order.ChangeStatus(request.Status);
        await repository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}
