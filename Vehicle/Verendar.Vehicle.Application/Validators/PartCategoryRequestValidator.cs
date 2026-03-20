namespace Verendar.Vehicle.Application.Validators
{
    public class PartCategoryRequestValidator : AbstractValidator<PartCategoryRequest>
    {
        public PartCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên danh mục không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên danh mục tối đa 100 ký tự");

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Mã danh mục không được để trống")
                .MaximumLength(50)
                .WithMessage("Mã danh mục tối đa 50 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Mô tả tối đa 500 ký tự");

            RuleFor(x => x.IconUrl)
                .MaximumLength(255)
                .When(x => !string.IsNullOrEmpty(x.IconUrl))
                .WithMessage("Đường dẫn icon tối đa 255 ký tự");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.IdentificationSigns)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.IdentificationSigns))
                .WithMessage("Dấu hiệu nhận biết tối đa 1000 ký tự");

            RuleFor(x => x.ConsequencesIfNotHandled)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.ConsequencesIfNotHandled))
                .WithMessage("Hậu quả nếu không xử lý tối đa 1000 ký tự");
        }
    }
}
