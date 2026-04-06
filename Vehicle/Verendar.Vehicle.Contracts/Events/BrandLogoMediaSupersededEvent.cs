using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events
{
    public class BrandLogoMediaSupersededEvent : BaseEvent
    {
        public override string EventType => "vehicle.brand.logo.superseded.v1";

        public Guid BrandId { get; set; }

        public Guid SupersededMediaFileId { get; set; }
    }
}
