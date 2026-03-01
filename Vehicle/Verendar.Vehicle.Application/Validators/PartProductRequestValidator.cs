using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class PartProductRequestValidator : AbstractValidator<PartProductRequest>
    {
        public PartProductRequestValidator()
        {
            RuleFor(x => x.PartCategoryId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn danh mục");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên sản phẩm không được để trống")
                .MaximumLength(200)
                .WithMessage("Tên sản phẩm tối đa 200 ký tự");

            RuleFor(x => x.Brand)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Brand))
                .WithMessage("Thương hiệu tối đa 100 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Mô tả tối đa 1000 ký tự");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Đường dẫn ảnh tối đa 500 ký tự");

            RuleFor(x => x.ReferencePrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ReferencePrice.HasValue)
                .WithMessage("Giá tham khảo phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.RecommendedKmInterval)
                .GreaterThan(0)
                .When(x => x.RecommendedKmInterval.HasValue)
                .WithMessage("Chu kỳ km khuyến nghị phải lớn hơn 0");

            RuleFor(x => x.RecommendedMonthsInterval)
                .GreaterThan(0)
                .When(x => x.RecommendedMonthsInterval.HasValue)
                .WithMessage("Chu kỳ tháng khuyến nghị phải lớn hơn 0");
        }
    }
}
