using System.ComponentModel.DataAnnotations;

namespace VMP.Identity.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password không được để trống")]
        [MinLength(8, ErrorMessage = "Password tối thiểu 8 ký tự")]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password không được để trống")]
        [MinLength(8, ErrorMessage = "Password tối thiểu 8 ký tự")]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token không được để trống")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu cũ tối thiểu 8 ký tự")]
        [MaxLength(100)]
        public string OldPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới tối thiểu 8 ký tự")]
        [MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
