using FluentValidation;
using Verendar.Identity.Application.Dtos;

namespace Verendar.Identity.Application.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token không được để trống");
        }
    }
}
