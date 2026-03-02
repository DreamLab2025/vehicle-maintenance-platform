using FluentValidation;
using Verendar.Identity.Application.Dtos;

namespace Verendar.Identity.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email không được để trống")
                .EmailAddress()
                .WithMessage("Email không hợp lệ")
                .MaximumLength(256)
                .WithMessage("Email tối đa 256 ký tự");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password không được để trống")
                .MinimumLength(8)
                .WithMessage("Password tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Password tối đa 100 ký tự");
        }
    }
}
