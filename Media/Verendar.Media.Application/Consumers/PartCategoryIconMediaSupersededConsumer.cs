using MassTransit;
using Verendar.Media.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Media.Application.Consumers
{
    public class PartCategoryIconMediaSupersededConsumer(
        IMediaUploadService mediaUploadService,
        ILogger<PartCategoryIconMediaSupersededConsumer> logger) : IConsumer<PartCategoryIconMediaSupersededEvent>
    {
        private readonly IMediaUploadService _mediaUploadService = mediaUploadService;
        private readonly ILogger<PartCategoryIconMediaSupersededConsumer> _logger = logger;

        public async Task Consume(ConsumeContext<PartCategoryIconMediaSupersededEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "PartCategoryIconMediaSuperseded: category {PartCategoryId}, superseded media {MediaFileId}",
                message.PartCategoryId, message.SupersededMediaFileId);

            await _mediaUploadService.ReleaseSupersededCatalogMediaAsync(
                message.SupersededMediaFileId,
                FileType.PartCategory,
                context.CancellationToken);
        }
    }
}
