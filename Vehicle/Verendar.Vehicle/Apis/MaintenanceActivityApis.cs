namespace Verendar.Vehicle.Apis
{
    public static class MaintenanceActivityApis
    {
        public static IEndpointRouteBuilder MapMaintenanceActivityApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/maintenance-activities")
                .MapMaintenanceActivityRoutes()
                .WithTags("Maintenance Activity Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }
        public static RouteGroupBuilder MapMaintenanceActivityRoutes(this RouteGroupBuilder group)
        {
            return group;
        }
    }
}
