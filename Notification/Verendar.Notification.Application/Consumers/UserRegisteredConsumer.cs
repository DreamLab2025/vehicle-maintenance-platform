using Verendar.Notification.Application.Mapping;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class UserRegisteredConsumer(
    ILogger<UserRegisteredConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        logger.LogInformation("Processing UserRegisteredEvent — MessageId: {MessageId}, UserId: {UserId}",
            messageId, message.UserId);

        try
        {
            if (!ValidateMessage(message))
            {
                logger.LogWarning("Invalid UserRegisteredEvent — MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
                return;
            }

            await unitOfWork.NotificationPreferences.AddAsync(message.UserRegisteredToPreferenceEntity());
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation(
                "UserRegisteredEvent processed (preference only) — MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing UserRegisteredEvent — MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
        }
    }

    private static bool ValidateMessage(UserRegisteredEvent message) =>
        !string.IsNullOrEmpty(message.FullName) && message.RegistrationDate != DateTime.MinValue;
}
