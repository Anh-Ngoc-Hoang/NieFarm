using FluentValidation;

namespace NieFarm.Application.Features.Admin.Activities.Commands;

public class SaveActivityCommandValidator : AbstractValidator<SaveActivityCommand>
{
    public SaveActivityCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .MaximumLength(220);

        RuleFor(x => x.ModalTitle)
            .MaximumLength(250);

        RuleFor(x => x.EventDate)
            .NotEqual(default(DateOnly)).WithMessage("Event date is required.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image is required.")
            .MaximumLength(500);

        RuleFor(x => x.ImageAlt)
            .NotEmpty().WithMessage("Image alt text is required.")
            .MaximumLength(300);
    }
}
