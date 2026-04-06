namespace Verendar.Identity.Application.Validators
{
    public class CreateMechanicRequestValidator : AbstractValidator<CreateMechanicRequest>
    {
        public CreateMechanicRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Họ tên không được để trống")
                .MaximumLength(200)
                .WithMessage("Họ tên tối đa 200 ký tự");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email không được để trống")
                .EmailAddress()
                .WithMessage("Email không hợp lệ")
                .MaximumLength(256)
                .WithMessage("Email tối đa 256 ký tự");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .WithMessage("Số điện thoại tối đa 20 ký tự")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Password)
                .MinimumLength(8)
                .WithMessage("Mật khẩu tối thiểu 8 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.Password));
        }
    }
}
