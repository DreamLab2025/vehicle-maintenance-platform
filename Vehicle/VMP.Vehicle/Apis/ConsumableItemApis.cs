namespace VMP.Vehicle.Apis
{
    public static class ConsumableItemApis
    {
        public static IEndpointRouteBuilder MapConsumableItemApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/consumable-items")
                .MapConsumableItemRoutes()
                .WithTags("Consumable Item Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapConsumableItemRoutes(this RouteGroupBuilder group)
        {

            return group;
        }
    }
}
