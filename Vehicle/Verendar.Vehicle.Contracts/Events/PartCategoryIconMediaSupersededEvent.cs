using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events
{
    public class PartCategoryIconMediaSupersededEvent : BaseEvent
    {
        public override string EventType => "vehicle.partcategory.icon.superseded.v1";

        public Guid PartCategoryId { get; set; }

        public Guid SupersededMediaFileId { get; set; }
    }
}
