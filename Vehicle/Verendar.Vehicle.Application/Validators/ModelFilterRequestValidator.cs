namespace Verendar.Vehicle.Application.Validators
{
    public class ModelFilterRequestValidator : AbstractValidator<ModelFilterRequest>
    {
        public ModelFilterRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Số trang phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Kích thước trang phải từ 1 đến 100");

            RuleFor(x => x.ModelName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.ModelName))
                .WithMessage("Tên mẫu xe tìm kiếm tối đa 200 ký tự");

            RuleFor(x => x.ReleaseYear)
                .InclusiveBetween(1900, 2100)
                .When(x => x.ReleaseYear.HasValue)
                .WithMessage("Năm sản xuất phải từ 1900 đến 2100");

            RuleFor(x => x.EngineDisplacement)
                .GreaterThan(0)
                .When(x => x.EngineDisplacement.HasValue)
                .WithMessage("Dung tích động cơ phải lớn hơn 0");
        }
    }
}
