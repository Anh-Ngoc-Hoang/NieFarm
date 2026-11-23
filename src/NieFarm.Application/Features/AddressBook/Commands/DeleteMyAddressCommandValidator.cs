using FluentValidation;

namespace NieFarm.Application.Features.AddressBook.Commands;

public class DeleteMyAddressCommandValidator : AbstractValidator<DeleteMyAddressCommand>
{
    public DeleteMyAddressCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage(AddressBookMessages.InvalidId);
    }
}
