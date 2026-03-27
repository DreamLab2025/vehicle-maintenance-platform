using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Services.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class MemberAccountCreatedConsumer(
        ILogger<MemberAccountCreatedConsumer> logger,
        IEmailNotificationService emailNotificationService,
        IHostEnvironment environment) : IConsumer<MemberAccountCreatedEvent>
    {
        private readonly ILogger<MemberAccountCreatedConsumer> _logger = logger;
        private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;
        private readonly IHostEnvironment _environment = environment;

        public async Task Consume(ConsumeContext<MemberAccountCreatedEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Processing MemberAccountCreatedEvent - MessageId: {MessageId}, UserId: {UserId}, Role: {Role}",
                messageId, message.UserId, message.Role);

            if (_environment.IsDevelopment())
            {
                _logger.LogInformation(
                    "[DEV] Member account created — UserId: {UserId}, Email: {Email}, Role: {Role}, TempPassword: {TempPassword}",
                    message.UserId, message.Email, message.Role, message.TempPassword);
                return;
            }

            try
            {
                var sent = await _emailNotificationService.SendMemberAccountCreatedEmailAsync(
                    message, context.CancellationToken);

                if (!sent)
                    _logger.LogWarning(
                        "Failed to send member account email - MessageId: {MessageId}, UserId: {UserId}",
                        messageId, message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing MemberAccountCreatedEvent - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
        }
    }
}
