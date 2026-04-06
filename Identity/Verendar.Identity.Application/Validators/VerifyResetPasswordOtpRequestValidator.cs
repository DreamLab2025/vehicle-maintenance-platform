namespace Verendar.Identity.Application.Validators
{
    public class VerifyResetPasswordOtpRequestValidator : AbstractValidator<VerifyResetPasswordOtpRequest>
    {
        public VerifyResetPasswordOtpRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email không được để trống")
                .EmailAddress()
                .WithMessage("Email không hợp lệ")
                .MaximumLength(256)
                .WithMessage("Email tối đa 256 ký tự");

            RuleFor(x => x.OtpCode)
                .NotEmpty()
                .WithMessage("Mã OTP không được để trống")
                .Length(6, 6)
                .WithMessage("Mã OTP phải bao gồm 6 ký tự")
                .Matches(@"^\d+$")
                .WithMessage("Mã OTP chỉ được chứa số");
        }
    }
}
