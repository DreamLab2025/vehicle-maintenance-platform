namespace Verendar.Vehicle.Application.Validators
{
    public class ModelRequestValidator : AbstractValidator<ModelRequest>
    {
        public ModelRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên mẫu xe không được để trống")
                .MaximumLength(200)
                .WithMessage("Tên mẫu xe tối đa 200 ký tự");

            RuleFor(x => x.Code)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.Code))
                .WithMessage("Mã mẫu xe tối đa 50 ký tự");

            RuleFor(x => x.BrandId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn thương hiệu");

            RuleFor(x => x.TypeId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn loại xe");

            RuleFor(x => x.ReleaseYear)
                .InclusiveBetween(1900, 2100)
                .When(x => x.ReleaseYear.HasValue)
                .WithMessage("Năm sản xuất phải từ 1900 đến 2100");

            RuleFor(x => x.EngineDisplacement)
                .GreaterThan(0)
                .When(x => x.EngineDisplacement.HasValue)
                .WithMessage("Dung tích động cơ (cc) phải lớn hơn 0");

            RuleFor(x => x.EngineCapacity)
                .GreaterThan(0)
                .When(x => x.EngineCapacity.HasValue)
                .WithMessage("Dung tích động cơ (lít) phải lớn hơn 0");

            RuleForEach(x => x.Images).ChildRules(img =>
            {
                img.RuleFor(x => x.Color)
                    .NotEmpty()
                    .WithMessage("Tên màu không được để trống")
                    .MaximumLength(100)
                    .WithMessage("Tên màu tối đa 100 ký tự");
                img.RuleFor(x => x.HexCode)
                    .NotEmpty()
                    .WithMessage("Mã màu Hex không được để trống")
                    .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
                    .WithMessage("Mã màu Hex không hợp lệ (vd: #FFF hoặc #FFFFFF)");
                img.RuleFor(x => x.ImageUrl)
                    .NotEmpty()
                    .WithMessage("URL hình ảnh màu không được để trống")
                    .MaximumLength(500)
                    .WithMessage("URL hình ảnh tối đa 500 ký tự");
            });
        }
    }
}
