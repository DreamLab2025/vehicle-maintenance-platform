using FluentValidation;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Đánh giá phải từ 1 đến 5 sao.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .When(x => x.Comment is not null)
            .WithMessage("Nhận xét không được quá 2000 ký tự.");
    }
}
