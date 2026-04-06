namespace Verendar.Vehicle.Application.Validators
{
    public class BrandRequestValidator : AbstractValidator<BrandRequest>
    {
        public BrandRequestValidator()
        {
            RuleFor(x => x.VehicleTypeId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn loại xe");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên thương hiệu không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên thương hiệu tối đa 100 ký tự");

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500)
                .WithMessage("Logo URL tối đa 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.LogoUrl));

            RuleFor(x => x)
                .Must(x => !x.LogoMediaFileId.HasValue || !string.IsNullOrWhiteSpace(x.LogoUrl))
                .WithMessage("LogoUrl là bắt buộc khi gửi LogoMediaFileId");
        }
    }
}
