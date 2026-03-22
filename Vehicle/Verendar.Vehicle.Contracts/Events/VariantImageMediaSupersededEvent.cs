using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events
{
    public class VariantImageMediaSupersededEvent : BaseEvent
    {
        public override string EventType => "vehicle.variant.image.superseded.v1";

        public Guid VariantId { get; set; }

        public Guid SupersededMediaFileId { get; set; }
    }
}
