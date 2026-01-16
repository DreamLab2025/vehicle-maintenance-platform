namespace Verender.Vehicle.Apis
{
    public static class OdometerHistoryApis
    {
        public static IEndpointRouteBuilder MapOdometerHistoryApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/odometer-history")
                .MapOdometerHistoryRoutes()
                .WithName("Odometer History Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapOdometerHistoryRoutes(this RouteGroupBuilder group)
        {

            return group;
        }
    }
}
