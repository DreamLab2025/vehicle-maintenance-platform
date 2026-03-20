using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class UserRegisteredConsumer(
        ILogger<UserRegisteredConsumer> logger,
        IUnitOfWork unitOfWork,
        IEmailNotificationService emailNotificationService) : IConsumer<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredConsumer> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogInformation("Processing UserRegisteredEvent - MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);

            try
            {
                if (!ValidateMessage(message))
                {
                    _logger.LogWarning("Invalid UserRegisteredEvent - MessageId: {MessageId}, UserId: {UserId}",
                        messageId, message.UserId);
                    return;
                }

                await _unitOfWork.NotificationPreferences.AddAsync(message.UserRegisteredToPreferenceEntity());
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                var sent = await _emailNotificationService.SendWelcomeEmailAsync(message, context.CancellationToken);

                if (sent)
                    _logger.LogInformation("Welcome email sent - MessageId: {MessageId}, UserId: {UserId}", messageId, message.UserId);
                else if (!string.IsNullOrWhiteSpace(message.Email))
                    _logger.LogWarning("Welcome email send failed - MessageId: {MessageId}, UserId: {UserId}", messageId, message.UserId);

                _logger.LogInformation("UserRegisteredEvent processed - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserRegisteredEvent - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
        }

        private static bool ValidateMessage(UserRegisteredEvent message) =>
            !string.IsNullOrEmpty(message.FullName) && message.RegistrationDate != DateTime.MinValue;
    }
}
