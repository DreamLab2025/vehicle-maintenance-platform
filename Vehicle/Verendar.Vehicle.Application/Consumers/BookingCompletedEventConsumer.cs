using MassTransit;
using Verendar.Garage.Contracts.Events;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Consumers
{
    public class BookingCompletedEventConsumer(
        ILogger<BookingCompletedEventConsumer> logger,
        IUnitOfWork unitOfWork) : IConsumer<BookingCompletedEvent>
    {
        private readonly ILogger<BookingCompletedEventConsumer> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Consume(ConsumeContext<BookingCompletedEvent> context)
        {
            var msg = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Processing BookingCompleted — MessageId:{MessageId} BookingId:{BookingId} VehicleId:{VehicleId}",
                messageId, msg.BookingId, msg.UserVehicleId);

            try
            {
                // Idempotent guard — consumer may be called more than once
                if (await _unitOfWork.MaintenanceProposals.ExistsByBookingIdAsync(msg.BookingId, context.CancellationToken))
                {
                    _logger.LogWarning(
                        "Proposal already exists for BookingId:{BookingId} — skipping",
                        msg.BookingId);
                    return;
                }

                var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(
                    v => v.Id == msg.UserVehicleId && v.DeletedAt == null);
                if (vehicle is null)
                {
                    _logger.LogWarning(
                        "UserVehicle {UserVehicleId} not found — skipping proposal creation for booking {BookingId}",
                        msg.UserVehicleId, msg.BookingId);
                    return;
                }

                var proposal = new MaintenanceProposal
                {
                    UserVehicleId = msg.UserVehicleId,
                    UserId = msg.UserId,
                    BookingId = msg.BookingId,
                    GarageBranchId = msg.GarageBranchId,
                    BranchName = msg.BranchName,
                    ServiceDate = DateOnly.FromDateTime(msg.CompletedAt),
                    OdometerAtService = msg.CurrentOdometer,
                    TotalAmount = msg.TotalAmount,
                    Status = ProposalStatus.Pending
                };

                foreach (var lineItem in msg.LineItems)
                {
                    proposal.Items.Add(new MaintenanceProposalItem
                    {
                        PartCategoryId = lineItem.PartCategoryId,
                        ItemName = lineItem.ItemName,
                        UpdatesTracking = lineItem.UpdatesTracking,
                        RecommendedKmInterval = lineItem.RecommendedKmInterval,
                        RecommendedMonthsInterval = lineItem.RecommendedMonthsInterval,
                        Price = lineItem.Price
                    });
                }

                await _unitOfWork.MaintenanceProposals.AddAsync(proposal);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation(
                    "MaintenanceProposal created — ProposalId:{ProposalId} BookingId:{BookingId} Items:{ItemCount}",
                    proposal.Id, msg.BookingId, proposal.Items.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating MaintenanceProposal — MessageId:{MessageId} BookingId:{BookingId}",
                    messageId, msg.BookingId);
                // Rethrow so MassTransit retry policy handles it
                throw;
            }
        }
    }
}
