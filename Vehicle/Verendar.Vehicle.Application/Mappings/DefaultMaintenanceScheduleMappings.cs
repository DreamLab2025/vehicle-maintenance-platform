namespace Verendar.Vehicle.Application.Mappings
{
    public static class DefaultMaintenanceScheduleMappings
    {
        public static DefaultScheduleResponse ToResponse(this DefaultMaintenanceSchedule schedule)
        {
            return new DefaultScheduleResponse
            {
                InitialKm = schedule.InitialKm,
                KmInterval = schedule.KmInterval,
                MonthsInterval = schedule.MonthsInterval,
                RequiresOdometerTracking = schedule.PartCategory.RequiresOdometerTracking,
                RequiresTimeTracking = schedule.PartCategory.RequiresTimeTracking
            };
        }

        public static List<DefaultScheduleResponse> ToResponseList(this IEnumerable<DefaultMaintenanceSchedule> schedules)
        {
            return schedules.Select(s => s.ToResponse()).ToList();
        }
    }
}
