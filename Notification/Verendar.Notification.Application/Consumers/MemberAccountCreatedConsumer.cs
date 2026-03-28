using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Verendar.Notification.Application.Options;
using Verendar.Notification.Application.Services.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class MemberAccountCreatedConsumer(
    ILogger<MemberAccountCreatedConsumer> logger,
    IEmailNotificationService emailNotificationService,
    IHostEnvironment environment,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<MemberAccountCreatedEvent>
{
    public async Task Consume(ConsumeContext<MemberAccountCreatedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        logger.LogInformation(
            "Processing MemberAccountCreatedEvent — MessageId: {MessageId}, UserId: {UserId}, Role: {Role}",
            messageId, message.UserId, message.Role);

        if (environment.IsDevelopment())
        {
            logger.LogInformation(
                "[DEV] Member account created — UserId: {UserId}, Email: {Email}, Role: {Role}, TempPassword: {TempPassword}",
                message.UserId, message.Email, message.Role, message.TempPassword);
            return;
        }

        try
        {
            var loginUrl = appOptions.Value.LoginAbsoluteUrl();
            var sent = await emailNotificationService.SendMemberAccountCreatedEmailAsync(
                message.Email,
                message.FullName,
                message.TempPassword,
                message.Role,
                loginUrl,
                context.CancellationToken);

            if (!sent)
            {
                logger.LogWarning(
                    "Failed to send member account email — MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing MemberAccountCreatedEvent — MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
        }
    }
}
