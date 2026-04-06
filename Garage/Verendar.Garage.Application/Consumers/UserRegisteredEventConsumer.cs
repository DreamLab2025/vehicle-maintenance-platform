using MassTransit;
using Microsoft.EntityFrameworkCore;
using Verender.Identity.Contracts.Events;
using Verendar.Garage.Contracts.Events;

namespace Verendar.Garage.Application.Consumers;

public class UserRegisteredEventConsumer(
    ILogger<UserRegisteredEventConsumer> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventConsumer> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.ReferralCode))
            return;

        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.ReferralCode == message.ReferralCode && g.DeletedAt == null);

        if (garage is null)
        {
            _logger.LogWarning("UserRegisteredEventConsumer: referral code not found: {Code}", message.ReferralCode);
            return;
        }

        if (await _unitOfWork.Referrals.ExistsAsync(garage.Id, message.UserId, context.CancellationToken))
        {
            _logger.LogWarning("UserRegisteredEventConsumer: duplicate referral ignored — garage {GarageId}, user {UserId}",
                garage.Id, message.UserId);
            return;
        }

        try
        {
            await _unitOfWork.Referrals.AddAsync(new GarageReferral
            {
                GarageId = garage.Id,
                ReferredUserId = message.UserId,
                ReferralCode = message.ReferralCode,
                ReferredAt = message.RegistrationDate
            });

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true ||
                                            ex.InnerException?.Message.Contains("duplicate") == true)
        {
            _logger.LogWarning("UserRegisteredEventConsumer: DB unique constraint hit — garage {GarageId}, user {UserId}",
                garage.Id, message.UserId);
            return;
        }

        await _publishEndpoint.Publish(new GarageReferralRecordedEvent
        {
            GarageId = garage.Id,
            GarageOwnerId = garage.OwnerId,
            BusinessName = garage.BusinessName,
            ReferredUserId = message.UserId,
            ReferralCode = message.ReferralCode,
            ReferredAt = message.RegistrationDate
        }, context.CancellationToken);

        _logger.LogInformation("UserRegisteredEventConsumer: referral recorded — garage {GarageId}, user {UserId}",
            garage.Id, message.UserId);
    }
}
