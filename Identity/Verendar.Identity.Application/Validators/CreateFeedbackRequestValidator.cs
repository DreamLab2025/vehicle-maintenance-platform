namespace Verendar.Identity.Application.Validators;

public class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
{
    public CreateFeedbackRequestValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Danh mục feedback không hợp lệ.");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Tiêu đề không được để trống.")
            .MinimumLength(3)
            .WithMessage("Tiêu đề phải có ít nhất 3 ký tự.")
            .MaximumLength(200)
            .WithMessage("Tiêu đề không được quá 200 ký tự.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung không được để trống.")
            .MinimumLength(10)
            .WithMessage("Nội dung phải có ít nhất 10 ký tự.")
            .MaximumLength(5000)
            .WithMessage("Nội dung không được quá 5000 ký tự.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Đánh giá phải từ 1 đến 5.")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .WithMessage("Email liên hệ không hợp lệ.")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ImageUrls)
            .Must(urls => urls == null || urls.Count <= 5)
            .WithMessage("Chỉ được đính kèm tối đa 5 ảnh.")
            .When(x => x.ImageUrls != null);

        RuleForEach(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("URL ảnh không được để trống.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("URL ảnh không hợp lệ.")
            .When(x => x.ImageUrls != null);
    }
}
