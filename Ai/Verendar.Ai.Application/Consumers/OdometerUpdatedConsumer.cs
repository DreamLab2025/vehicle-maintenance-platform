using MassTransit;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Ai.Application.Consumers
{
    public class OdometerUpdatedConsumer(
        IReAnalysisService reAnalysisService,
        ILogger<OdometerUpdatedConsumer> logger) : IConsumer<OdometerUpdatedEvent>
    {
        private readonly IReAnalysisService _reAnalysisService = reAnalysisService;
        private readonly ILogger<OdometerUpdatedConsumer> _logger = logger;

        public async Task Consume(ConsumeContext<OdometerUpdatedEvent> context)
        {
            var message = context.Message;

            if (message.TotalEntryCount < 2)
            {
                _logger.LogDebug(
                    "OdometerUpdatedConsumer: skip vehicle {VehicleId} — entryCount {Count} < 2",
                    message.UserVehicleId, message.TotalEntryCount);
                return;
            }

            _logger.LogInformation(
                "OdometerUpdatedConsumer: queuing re-analysis for vehicle {VehicleId} (entryCount={Count})",
                message.UserVehicleId, message.TotalEntryCount);

            await _reAnalysisService.QueueReAnalysisForBaselinePartsAsync(message.UserVehicleId, message.UserId);
        }
    }
}
