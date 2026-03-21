namespace Verendar.Identity.Application.Validators
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu cũ không được để trống")
                .MinimumLength(8)
                .WithMessage("Mật khẩu cũ tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Mật khẩu cũ tối đa 100 ký tự");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(8)
                .WithMessage("Mật khẩu mới tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Mật khẩu mới tối đa 100 ký tự");
        }
    }
}
