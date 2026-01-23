using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Verendar.Vehicle.Apis
{
    public static class VehiclePartApis
    {
        public static IEndpointRouteBuilder MapVehiclePartApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/vehicle-parts")
                .MapVehiclePartRoutes()
                .WithTags("Vehicle Part Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapVehiclePartRoutes(this RouteGroupBuilder group)
        {
            // TODO: Implement endpoints
            return group;
        }
    }
}
