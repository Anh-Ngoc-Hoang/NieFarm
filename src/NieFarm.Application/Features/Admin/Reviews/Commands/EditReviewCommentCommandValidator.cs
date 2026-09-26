using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

public class EditReviewCommentCommandValidator : AbstractValidator<EditReviewCommentCommand>
{
    public EditReviewCommentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MinimumLength(Review.CommentMinLength)
                .WithMessage($"Comment must be at least {Review.CommentMinLength} characters.")
            .MaximumLength(Review.CommentMaxLength)
                .WithMessage($"Comment must be at most {Review.CommentMaxLength} characters.");
    }
}
