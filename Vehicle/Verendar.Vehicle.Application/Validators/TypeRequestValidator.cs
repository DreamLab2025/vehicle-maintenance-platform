namespace Verendar.Vehicle.Application.Validators
{
    public class TypeRequestValidator : AbstractValidator<TypeRequest>
    {
        public TypeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên loại xe không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên loại xe tối đa 100 ký tự");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .WithMessage("URL hình ảnh tối đa 500 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả tối đa 500 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
