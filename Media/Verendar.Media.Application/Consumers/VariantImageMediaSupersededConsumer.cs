using MassTransit;
using Verendar.Media.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Media.Application.Consumers
{
    public class VariantImageMediaSupersededConsumer(
        IMediaUploadService mediaUploadService,
        ILogger<VariantImageMediaSupersededConsumer> logger) : IConsumer<VariantImageMediaSupersededEvent>
    {
        private readonly IMediaUploadService _mediaUploadService = mediaUploadService;
        private readonly ILogger<VariantImageMediaSupersededConsumer> _logger = logger;

        public async Task Consume(ConsumeContext<VariantImageMediaSupersededEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "VariantImageMediaSuperseded: variant {VariantId}, superseded media {MediaFileId}",
                message.VariantId, message.SupersededMediaFileId);

            await _mediaUploadService.ReleaseSupersededCatalogMediaAsync(
                message.SupersededMediaFileId,
                FileType.VehicleVariant,
                context.CancellationToken);
        }
    }
}
