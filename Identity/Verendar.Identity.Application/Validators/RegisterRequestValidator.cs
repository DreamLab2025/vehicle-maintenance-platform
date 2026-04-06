namespace Verendar.Identity.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
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

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password không được để trống")
                .MinimumLength(8)
                .WithMessage("Password tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Password tối đa 100 ký tự")
                .Matches(@"[A-Z]")
                .WithMessage("Password phải chứa ít nhất 1 ký tự viết hoa")
                .Matches(@"[a-z]")
                .WithMessage("Password phải chứa ít nhất 1 ký tự viết thường")
                .Matches(@"\d")
                .WithMessage("Password phải chứa ít nhất 1 chữ số");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Số điện thoại không được để trống")
                .Matches(@"^0[3-9]\d{8}$")
                .WithMessage("Số điện thoại không hợp lệ (phải là số Việt Nam 10 chữ số)");

            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob == null || dob.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-16)))
                .WithMessage("Người dùng phải đủ 16 tuổi")
                .Must(dob => dob == null || dob.Value >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-100)))
                .WithMessage("Ngày sinh không hợp lệ");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .When(x => x.Gender.HasValue)
                .WithMessage("Giới tính không hợp lệ");
        }
    }
}
