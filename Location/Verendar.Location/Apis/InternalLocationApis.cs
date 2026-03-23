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
            .Produces(422)
            .Produces(StatusCodes.Status500InternalServerError);

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

        var province = await provinceService.GetProvinceByCodeAsync(provinceCode.Trim());
        if (!province.IsSuccess)
        {
            if (province.StatusCode == StatusCodes.Status404NotFound)
                return Results.UnprocessableEntity(new { isValid = false, errors = new[] { "Province not found" } });

            return Results.Problem(
                detail: province.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        if (province.Data == null)
        {
            return Results.Problem(
                detail: "Province response was empty",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        if (!string.IsNullOrWhiteSpace(wardCode))
        {
            var ward = await wardService.GetWardByCodeAsync(wardCode.Trim());
            if (!ward.IsSuccess)
            {
                if (ward.StatusCode == StatusCodes.Status404NotFound)
                    return Results.UnprocessableEntity(new { isValid = false, errors = new[] { "Ward not found" } });

                return Results.Problem(
                    detail: ward.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            if (ward.Data == null)
            {
                return Results.Problem(
                    detail: "Ward response was empty",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            if (!string.Equals(ward.Data.ProvinceCode, province.Data.Code, StringComparison.OrdinalIgnoreCase))
            {
                return Results.UnprocessableEntity(new
                {
                    isValid = false,
                    errors = new[]
                    {
                        $"Ward {wardCode} belongs to province {ward.Data.ProvinceCode}, not {province.Data.Code}"
                    }
                });
            }

            return Results.Ok(new { isValid = true, provinceName = province.Data.Name, wardName = ward.Data.Name });
        }

        return Results.Ok(new { isValid = true, provinceName = province.Data.Name });
    }
}
