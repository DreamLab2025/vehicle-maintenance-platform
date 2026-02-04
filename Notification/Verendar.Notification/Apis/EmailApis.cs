using Verendar.Common.Shared;
using Verendar.Notification.Application.Dtos.Email;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Apis;

public static class EmailApis
{
    public static IEndpointRouteBuilder MapEmailApis(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/emails")
            .WithTags("Email API")
            .RequireRateLimiting("Fixed");

        // Send email with template
        group.MapPost("/send", SendEmail)
            .WithName("SendEmail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gửi email với template";
                operation.Description = "Gửi email cho user sử dụng template có sẵn (Otp, Welcome, PasswordReset, Notification)";
                return operation;
            })
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status400BadRequest);

        // Send OTP email
        group.MapPost("/send-otp", SendOtpEmail)
            .WithName("SendOtpEmail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gửi email OTP";
                operation.Description = "Gửi email chứa mã OTP cho user";
                return operation;
            })
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status400BadRequest);

        // Send welcome email
        group.MapPost("/send-welcome", SendWelcomeEmail)
            .WithName("SendWelcomeEmail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gửi email chào mừng";
                operation.Description = "Gửi email chào mừng cho user mới đăng ký";
                return operation;
            })
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status400BadRequest);

        // Send password reset email
        group.MapPost("/send-password-reset", SendPasswordResetEmail)
            .WithName("SendPasswordResetEmail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gửi email đặt lại mật khẩu";
                operation.Description = "Gửi email chứa link đặt lại mật khẩu";
                return operation;
            })
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status400BadRequest);

        // Send custom notification email
        group.MapPost("/send-notification", SendNotificationEmail)
            .WithName("SendNotificationEmail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gửi email thông báo tùy chỉnh";
                operation.Description = "Gửi email thông báo với nội dung tùy chỉnh";
                return operation;
            })
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SendEmailResponse>>(StatusCodes.Status400BadRequest);

        return builder;
    }

    private static async Task<IResult> SendEmail(
        SendEmailRequest request,
        IResendEmailService emailService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Email người nhận không được để trống"));
            }

            if (string.IsNullOrWhiteSpace(request.TemplateKey))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Template key không được để trống"));
            }

            object? model = request.TemplateKey.ToLower() switch
            {
                "otp" => new OtpEmailModel
                {
                    OtpCode = request.Parameters?.GetValueOrDefault("OtpCode") ?? string.Empty,
                    ExpiryMinutes = int.TryParse(request.Parameters?.GetValueOrDefault("ExpiryMinutes"), out var exp) ? exp : 10,
                    ExpiryTime = DateTime.TryParse(request.Parameters?.GetValueOrDefault("ExpiryTime"), out var expTime)
                        ? expTime : DateTime.UtcNow.AddMinutes(10),
                    OtpType = request.Parameters?.GetValueOrDefault("OtpType") ?? "Verification",
                    UserName = request.Parameters?.GetValueOrDefault("UserName") ?? "User",
                    UserEmail = request.To
                },
                "welcome" => new WelcomeEmailModel
                {
                    FullName = request.Parameters?.GetValueOrDefault("FullName") ?? "User",
                    RegistrationDate = DateTime.TryParse(request.Parameters?.GetValueOrDefault("RegistrationDate"), out var regDate)
                        ? regDate : DateTime.UtcNow,
                    WelcomeMessage = request.Parameters?.GetValueOrDefault("WelcomeMessage"),
                    UserName = request.Parameters?.GetValueOrDefault("UserName") ?? "User",
                    UserEmail = request.To
                },
                "passwordreset" => new PasswordResetEmailModel
                {
                    ResetToken = request.Parameters?.GetValueOrDefault("ResetToken") ?? string.Empty,
                    ResetUrl = request.Parameters?.GetValueOrDefault("ResetUrl") ?? string.Empty,
                    ExpiryMinutes = int.TryParse(request.Parameters?.GetValueOrDefault("ExpiryMinutes"), out var exp) ? exp : 60,
                    ExpiryTime = DateTime.TryParse(request.Parameters?.GetValueOrDefault("ExpiryTime"), out var expTime)
                        ? expTime : DateTime.UtcNow.AddHours(1),
                    UserName = request.Parameters?.GetValueOrDefault("UserName") ?? "User",
                    UserEmail = request.To
                },
                "notification" => new NotificationEmailModel
                {
                    Title = request.Parameters?.GetValueOrDefault("Title") ?? "Thông báo",
                    Message = request.Parameters?.GetValueOrDefault("Message") ?? string.Empty,
                    ActionUrl = request.Parameters?.GetValueOrDefault("ActionUrl"),
                    ActionText = request.Parameters?.GetValueOrDefault("ActionText"),
                    UserName = request.Parameters?.GetValueOrDefault("UserName") ?? "User",
                    UserEmail = request.To
                },
                _ => new NotificationEmailModel
                {
                    Title = "Thông báo",
                    Message = request.Parameters?.GetValueOrDefault("Message") ?? string.Empty,
                    UserName = request.Parameters?.GetValueOrDefault("UserName") ?? "User",
                    UserEmail = request.To
                }
            };

            var result = await emailService.SendTemplatedEmailAsync(
                request.To,
                request.TemplateKey,
                "Thông báo từ Verendar",
                model);

            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse<SendEmailResponse>.SuccessResponse(
                    new SendEmailResponse
                    {
                        MessageId = result.MessageId,
                        SentAt = DateTime.UtcNow
                    },
                    "Email đã được gửi thành công"));
            }

            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Gửi email thất bại: {result.ErrorMessage}"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Lỗi: {ex.Message}"));
        }
    }

    private static async Task<IResult> SendOtpEmail(
        SendOtpEmailRequest request,
        IResendEmailService emailService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Email người nhận không được để trống"));
            }

            // Generate OTP code tự động (6 digits)
            var otpCode = GenerateOtpCode();
            var expiryMinutes = 10;
            var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var model = new OtpEmailModel
            {
                OtpCode = otpCode,
                ExpiryMinutes = expiryMinutes,
                ExpiryTime = expiryTime,
                OtpType = "Verification",
                UserName = "User",
                UserEmail = request.To
            };

            var result = await emailService.SendTemplatedEmailAsync(
                request.To,
                "Otp",
                "Mã xác thực OTP của bạn",
                model);

            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse<SendEmailResponse>.SuccessResponse(
                    new SendEmailResponse
                    {
                        MessageId = result.MessageId,
                        SentAt = DateTime.UtcNow
                    },
                    "Email OTP đã được gửi thành công"));
            }

            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Gửi email OTP thất bại: {result.ErrorMessage}"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Lỗi: {ex.Message}"));
        }
    }

    private static async Task<IResult> SendWelcomeEmail(
        SendWelcomeEmailRequest request,
        IResendEmailService emailService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Email người nhận không được để trống"));
            }

            var model = new WelcomeEmailModel
            {
                FullName = request.FullName ?? "User",
                RegistrationDate = DateTime.UtcNow,
                WelcomeMessage = null,
                UserName = request.FullName ?? "User",
                UserEmail = request.To
            };

            var result = await emailService.SendTemplatedEmailAsync(
                request.To,
                "Welcome",
                "Chào mừng đến với Verendar",
                model);

            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse<SendEmailResponse>.SuccessResponse(
                    new SendEmailResponse
                    {
                        MessageId = result.MessageId,
                        SentAt = DateTime.UtcNow
                    },
                    "Email chào mừng đã được gửi thành công"));
            }

            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Gửi email chào mừng thất bại: {result.ErrorMessage}"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Lỗi: {ex.Message}"));
        }
    }

    private static async Task<IResult> SendPasswordResetEmail(
        SendPasswordResetEmailRequest request,
        IResendEmailService emailService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Email người nhận không được để trống"));
            }

            if (string.IsNullOrWhiteSpace(request.ResetUrl))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "ResetUrl phải được cung cấp"));
            }

            var model = new PasswordResetEmailModel
            {
                ResetToken = string.Empty,
                ResetUrl = request.ResetUrl,
                ExpiryMinutes = 60,
                ExpiryTime = DateTime.UtcNow.AddHours(1),
                UserName = "User",
                UserEmail = request.To
            };

            var result = await emailService.SendTemplatedEmailAsync(
                request.To,
                "PasswordReset",
                "Đặt lại mật khẩu",
                model);

            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse<SendEmailResponse>.SuccessResponse(
                    new SendEmailResponse
                    {
                        MessageId = result.MessageId,
                        SentAt = DateTime.UtcNow
                    },
                    "Email đặt lại mật khẩu đã được gửi thành công"));
            }

            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Gửi email đặt lại mật khẩu thất bại: {result.ErrorMessage}"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Lỗi: {ex.Message}"));
        }
    }

    private static async Task<IResult> SendNotificationEmail(
        SendNotificationEmailRequest request,
        IResendEmailService emailService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Email người nhận không được để trống"));
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                    "Nội dung thông báo không được để trống"));
            }

            var model = new NotificationEmailModel
            {
                Title = request.Title,
                Message = request.Message,
                ActionUrl = request.ActionUrl,
                ActionText = null,
                UserName = "User",
                UserEmail = request.To
            };

            var result = await emailService.SendTemplatedEmailAsync(
                request.To,
                "Notification",
                request.Title,
                model);

            if (result.IsSuccess)
            {
                return Results.Ok(ApiResponse<SendEmailResponse>.SuccessResponse(
                    new SendEmailResponse
                    {
                        MessageId = result.MessageId,
                        SentAt = DateTime.UtcNow
                    },
                    "Email thông báo đã được gửi thành công"));
            }

            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Gửi email thông báo thất bại: {result.ErrorMessage}"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<SendEmailResponse>.FailureResponse(
                $"Lỗi: {ex.Message}"));
        }
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

// Request/Response DTOs
public class SendEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty; // Otp, Welcome, PasswordReset, Notification
    public Dictionary<string, string>? Parameters { get; set; }
}

public class SendOtpEmailRequest
{
    public string To { get; set; } = string.Empty;
}

public class SendWelcomeEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class SendPasswordResetEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string ResetUrl { get; set; } = string.Empty;
}

public class SendNotificationEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Title { get; set; } = "Thông báo";
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
}

public class SendEmailResponse
{
    public string? MessageId { get; set; }
    public DateTime SentAt { get; set; }
}
