namespace Verendar.Vehicle.Application.Validators
{
    public class VehicleVariantUpdateRequestValidator : AbstractValidator<VehicleVariantUpdateRequest>
    {
        public VehicleVariantUpdateRequestValidator()
        {
            RuleFor(x => x.Color)
                .NotEmpty()
                .WithMessage("Tên màu không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên màu tối đa 100 ký tự");

            RuleFor(x => x.HexCode)
                .NotEmpty()
                .WithMessage("Mã màu Hex không được để trống")
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
                .WithMessage("Mã màu Hex không hợp lệ (vd: #FFF hoặc #FFFFFF)");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("URL hình ảnh không được để trống")
                .MaximumLength(500)
                .WithMessage("URL hình ảnh tối đa 500 ký tự");
        }
    }
}
