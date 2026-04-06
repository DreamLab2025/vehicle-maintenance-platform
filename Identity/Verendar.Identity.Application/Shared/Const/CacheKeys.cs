namespace Verendar.Identity.Application.Shared.Const;

public static class CacheKeys
{
    private const string OtpRegisterSegment = "otp_register";
    private const string OtpForgotSegment = "otp_forgot";
    private const string OtpResendLockSegment = "otp_resend_lock";
    private const string OtpForgotLockSegment = "otp_forgot_lock";
    private const string OtpForgotVerifiedSegment = "otp_forgot_verified";

    public static string OtpRegister(string email) => $"{OtpRegisterSegment}:{email}";

    public static string OtpForgot(string email) => $"{OtpForgotSegment}:{email}";

    public static string OtpForgotVerified(string email) => $"{OtpForgotVerifiedSegment}:{email}";

    public static string OtpResendLock(string email) => $"{OtpResendLockSegment}:{email}";

    public static string OtpForgotLock(string email) => $"{OtpForgotLockSegment}:{email}";

    public static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    public static readonly TimeSpan OtpForgotVerifiedTtl = TimeSpan.FromMinutes(10);

    public static readonly TimeSpan OtpActionLockTtl = TimeSpan.FromSeconds(60);

    private const string ReferralCodeSegment = "referral_code";
    public static string ReferralCode(string email) => $"{ReferralCodeSegment}:{email}";
    public static readonly TimeSpan ReferralCodeTtl = TimeSpan.FromMinutes(10);
}
