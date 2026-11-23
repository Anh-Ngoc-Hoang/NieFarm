using FluentValidation;

namespace NieFarm.Application.Features.AddressBook.Commands;

public class SetDefaultMyAddressCommandValidator : AbstractValidator<SetDefaultMyAddressCommand>
{
    public SetDefaultMyAddressCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage(AddressBookMessages.InvalidId);
    }
}
