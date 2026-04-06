using Microsoft.AspNetCore.Mvc;
using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Vehicle.Apis;

public static class InternalGarageVehicleApis
{
    public static IEndpointRouteBuilder MapInternalGarageVehicleApi(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/internal/garage")
            .WithTags("Internal Garage Vehicle")
            .RequireAuthorization(policy => policy.RequireRole("Service"));

        group.MapGet("/user-vehicles/{userVehicleId:guid}", GetUserVehicleForGarage)
            .WithName("InternalGetUserVehicleForGarage")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Garage xem xe khách (Service JWT — ownerUserId = chủ xe)";
                return operation;
            })
            .Produces<ApiResponse<GaragePartnerUserVehicleDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GaragePartnerUserVehicleDto>>(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<IResult> GetUserVehicleForGarage(
        Guid userVehicleId,
        [FromQuery] Guid ownerUserId,
        IUserVehicleService vehicleService,
        CancellationToken cancellationToken)
    {
        var result = await vehicleService.GetUserVehicleForGaragePartnerAsync(ownerUserId, userVehicleId, cancellationToken);
        return result.ToHttpResult();
    }
}
