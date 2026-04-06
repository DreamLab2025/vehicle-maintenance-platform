using MassTransit;
using Verendar.Media.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Media.Application.Consumers
{
    public class BrandLogoMediaSupersededConsumer(
        IMediaUploadService mediaUploadService,
        ILogger<BrandLogoMediaSupersededConsumer> logger) : IConsumer<BrandLogoMediaSupersededEvent>
    {
        private readonly IMediaUploadService _mediaUploadService = mediaUploadService;
        private readonly ILogger<BrandLogoMediaSupersededConsumer> _logger = logger;

        public async Task Consume(ConsumeContext<BrandLogoMediaSupersededEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "BrandLogoMediaSuperseded: brand {BrandId}, superseded media {MediaFileId}",
                message.BrandId, message.SupersededMediaFileId);

            await _mediaUploadService.ReleaseSupersededCatalogMediaAsync(
                message.SupersededMediaFileId,
                FileType.VehicleBrand,
                context.CancellationToken);
        }
    }
}
