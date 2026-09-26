using FluentValidation;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Reviews.Commands;

public class EditProductReviewCommandValidator : AbstractValidator<EditProductReviewCommand>
{
    public EditProductReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).GreaterThan(0);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Vui lòng chọn số sao từ 1 đến 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Vui lòng nhập nội dung đánh giá.")
            .MinimumLength(Review.CommentMinLength)
                .WithMessage($"Nội dung đánh giá phải có ít nhất {Review.CommentMinLength} ký tự.")
            .MaximumLength(Review.CommentMaxLength)
                .WithMessage($"Nội dung đánh giá không vượt quá {Review.CommentMaxLength} ký tự.");
    }
}
