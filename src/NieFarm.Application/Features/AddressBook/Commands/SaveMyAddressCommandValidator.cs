using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Commands;

public class SaveMyAddressCommandValidator : AbstractValidator<SaveMyAddressCommand>
{
    public SaveMyAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(AddressBookMessages.InvalidId)
            .When(x => x.Id is not null);

        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage(AddressBookMessages.RecipientRequired)
            .MaximumLength(CustomerAddress.RecipientNameMaxLength).WithMessage(AddressBookMessages.RecipientTooLong);

        // Same limits and pattern as CreateOrderCommandValidator, so a saved address always orders.
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(AddressBookMessages.PhoneRequired)
            .MaximumLength(CustomerAddress.PhoneMaxLength).WithMessage(AddressBookMessages.PhoneInvalid)
            .Matches(@"^[0-9+\-\s().]{8,20}$").WithMessage(AddressBookMessages.PhoneInvalid);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage(AddressBookMessages.StreetRequired)
            .MaximumLength(CustomerAddress.StreetMaxLength).WithMessage(AddressBookMessages.StreetTooLong);

        RuleFor(x => x.WardName)
            .NotEmpty().WithMessage(AddressBookMessages.WardRequired)
            .MaximumLength(CustomerAddress.WardNameMaxLength).WithMessage(AddressBookMessages.WardTooLong);

        RuleFor(x => x.ProvinceName)
            .Must((cmd, _) => AddressProvinceResolver.Resolve(cmd.ProvinceCode, cmd.ProvinceName) is not null)
            .WithMessage(AddressBookMessages.ProvinceInvalid);

        RuleFor(x => x.WardCode)
            .GreaterThan(0).WithMessage(AddressBookMessages.WardRequired)
            .When(x => x.WardCode is not null);
    }
}
