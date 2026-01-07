namespace VMP.Vehicle.Apis
{
    public static class SMSchedule
    {
        public static IEndpointRouteBuilder MapSMScheduleApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/maintenance-schedules")
                .MapSMScheduleRoutes()
                .WithTags("Standard Maintenance Schedule Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }
        public static RouteGroupBuilder MapSMScheduleRoutes(this RouteGroupBuilder group)
        {
            return group;
        }
    }
}
