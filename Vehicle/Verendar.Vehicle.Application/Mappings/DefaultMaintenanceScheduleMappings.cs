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
                Id = schedule.Id,
                PartCategoryId = schedule.PartCategoryId,
                PartCategoryCode = schedule.PartCategory.Code,
                PartCategoryName = schedule.PartCategory.Name,
                PartCategoryDescription = schedule.PartCategory.Description,
                IconUrl = schedule.PartCategory.IconUrl,
                InitialKm = schedule.InitialKm,
                KmInterval = schedule.KmInterval,
                MonthsInterval = schedule.MonthsInterval,
                RequiresOdometerTracking = schedule.PartCategory.RequiresOdometerTracking,
                RequiresTimeTracking = schedule.PartCategory.RequiresTimeTracking,
                DisplayOrder = schedule.PartCategory.DisplayOrder
            };
        }

        public static List<DefaultMaintenanceScheduleResponse> ToResponseList(this IEnumerable<DefaultMaintenanceSchedule> schedules)
        {
            return schedules.Select(s => s.ToResponse()).ToList();
        }
    }
}
