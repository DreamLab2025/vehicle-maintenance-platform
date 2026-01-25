using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class DefaultMaintenanceScheduleMappings
    {
        public static DefaultMaintenanceScheduleResponse ToResponse(this DefaultMaintenanceSchedule schedule)
        {
            return new DefaultMaintenanceScheduleResponse
            {
                InitialKm = schedule.InitialKm,
                KmInterval = schedule.KmInterval,
                MonthsInterval = schedule.MonthsInterval,
                RequiresOdometerTracking = schedule.PartCategory.RequiresOdometerTracking,
                RequiresTimeTracking = schedule.PartCategory.RequiresTimeTracking
            };
        }

        public static List<DefaultMaintenanceScheduleResponse> ToResponseList(this IEnumerable<DefaultMaintenanceSchedule> schedules)
        {
            return schedules.Select(s => s.ToResponse()).ToList();
        }
    }
}
