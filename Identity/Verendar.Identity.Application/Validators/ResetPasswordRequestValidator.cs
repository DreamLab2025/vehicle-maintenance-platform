namespace Verendar.Identity.Application.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
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
                .WithMessage("Mã OTP phải gồm 6 chữ số");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(8)
                .WithMessage("Mật khẩu tối thiểu 8 ký tự");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Mật khẩu xác nhận không khớp");
        }
    }
}
