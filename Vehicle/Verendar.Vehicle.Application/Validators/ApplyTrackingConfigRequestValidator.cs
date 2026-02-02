using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class ApplyTrackingConfigRequestValidator : AbstractValidator<ApplyTrackingConfigRequest>
    {
        public ApplyTrackingConfigRequestValidator()
        {
            RuleFor(x => x.PartCategoryCode)
                .NotEmpty()
                .WithMessage("Mã linh kiện không được để trống")
                .MaximumLength(50)
                .WithMessage("Mã linh kiện tối đa 50 ký tự");

            RuleFor(x => x.LastReplacementOdometer)
                .GreaterThanOrEqualTo(0)
                .When(x => x.LastReplacementOdometer.HasValue)
                .WithMessage("Số km thay thế lần cuối phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.PredictedNextOdometer)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PredictedNextOdometer.HasValue)
                .WithMessage("Số km dự đoán lần tiếp theo phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.ConfidenceScore)
                .InclusiveBetween(0, 1)
                .When(x => x.ConfidenceScore.HasValue)
                .WithMessage("Độ tin cậy phải từ 0 đến 1");
        }
    }
}
