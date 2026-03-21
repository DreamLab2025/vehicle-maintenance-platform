namespace Verendar.Location.Apis;

public static class InternalLocationApis
{
    public static IEndpointRouteBuilder MapInternalLocationApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/internal/locations")
            .MapInternalLocationRoutes();
        return builder;
    }

    public static RouteGroupBuilder MapInternalLocationRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/validate", ValidateLocation)
            .WithName("ValidateLocation")
            .Produces(200)
            .Produces(422);

        return group;
    }

    private static async Task<IResult> ValidateLocation(
        string? provinceCode,
        string? wardCode,
        IProvinceService provinceService,
        IWardService wardService)
    {
        if (string.IsNullOrWhiteSpace(provinceCode))
            return Results.UnprocessableEntity(new { isValid = false, errors = new[] { "Province code is required" } });

        var province = await provinceService.GetProvinceByCodeAsync(provinceCode);
        if (province?.Data == null)
            return Results.UnprocessableEntity(new { isValid = false, errors = new[] { "Province not found" } });

        if (!string.IsNullOrWhiteSpace(wardCode))
        {
            var ward = await wardService.GetWardByCodeAsync(wardCode);
            if (ward?.Data == null)
                return Results.UnprocessableEntity(new { isValid = false, errors = new[] { "Ward not found" } });

            if (ward.Data.ProvinceCode != provinceCode)
                return Results.UnprocessableEntity(new { isValid = false, errors = new[] { $"Ward {wardCode} belongs to province {ward.Data.ProvinceCode}, not {provinceCode}" } });

            return Results.Ok(new { isValid = true, provinceName = province.Data.Name, wardName = ward.Data.Name });
        }

        return Results.Ok(new { isValid = true, provinceName = province.Data.Name });
    }
}
