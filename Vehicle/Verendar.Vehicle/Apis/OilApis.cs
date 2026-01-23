using Microsoft.AspNetCore.Mvc;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Apis
{
    public static class OilApis
    {
        public static IEndpointRouteBuilder MapOilApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/oils")
                .MapOilRoutes()
                .WithTags("Oil Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapOilRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllOils)
                .WithName("GetAllOils")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả nhớt";
                    return operation;
                })
                .Produces<ApiResponse<List<OilResponse>>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", GetOilById)
                .WithName("GetOilById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin nhớt theo ID";
                    return operation;
                })
                .Produces<ApiResponse<OilResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<OilResponse>>(StatusCodes.Status404NotFound);

            group.MapGet("/vehicle-part/{vehiclePartId:guid}", GetOilByVehiclePartId)
                .WithName("GetOilByVehiclePartId")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin nhớt theo VehiclePartId";
                    return operation;
                })
                .Produces<ApiResponse<OilResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<OilResponse>>(StatusCodes.Status404NotFound);

            group.MapGet("/usage/{vehicleUsage:int}", GetOilsByVehicleUsage)
                .WithName("GetOilsByVehicleUsage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách nhớt theo loại xe (1=Xe ga, 2=Xe số, 3=Cả hai)";
                    return operation;
                })
                .Produces<ApiResponse<List<OilResponse>>>(StatusCodes.Status200OK);

            group.MapGet("/viscosity/{viscosityGrade}", GetOilsByViscosityGrade)
                .WithName("GetOilsByViscosityGrade")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách nhớt theo cấp độ nhớt (ví dụ: 5W-30)";
                    return operation;
                })
                .Produces<ApiResponse<List<OilResponse>>>(StatusCodes.Status200OK);

            group.MapPost("/", CreateOil)
                .WithName("CreateOil")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo thông tin nhớt mới";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<OilResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPut("/{id:guid}", UpdateOil)
                .WithName("UpdateOil")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật thông tin nhớt";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<OilResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", DeleteOil)
                .WithName("DeleteOil")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa thông tin nhớt";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> GetAllOils(
            [AsParameters] PaginationRequest paginationRequest,
            IOilService oilService)
        {
            var result = await oilService.GetAllOilsAsync(paginationRequest);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetOilById(
            Guid id,
            IOilService oilService)
        {
            var result = await oilService.GetOilByIdAsync(id);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.NotFound(result);
        }

        private static async Task<IResult> GetOilByVehiclePartId(
            Guid vehiclePartId,
            IOilService oilService)
        {
            var result = await oilService.GetOilByVehiclePartIdAsync(vehiclePartId);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.NotFound(result);
        }

        private static async Task<IResult> GetOilsByVehicleUsage(
            int vehicleUsage,
            IOilService oilService)
        {
            if (!Enum.IsDefined(typeof(OilVehicleUsage), vehicleUsage))
            {
                return Results.BadRequest(ApiResponse<List<OilResponse>>.FailureResponse("Loại xe không hợp lệ"));
            }

            var result = await oilService.GetOilsByVehicleUsageAsync((OilVehicleUsage)vehicleUsage);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetOilsByViscosityGrade(
            string viscosityGrade,
            IOilService oilService)
        {
            var result = await oilService.GetOilsByViscosityGradeAsync(viscosityGrade);
            return Results.Ok(result);
        }

        private static async Task<IResult> CreateOil(
            OilRequest request,
            IOilService oilService)
        {
            var result = await oilService.CreateOilAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateOil(
            Guid id,
            OilRequest request,
            IOilService oilService)
        {
            var result = await oilService.UpdateOilAsync(id, request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.NotFound(result);
        }

        private static async Task<IResult> DeleteOil(
            Guid id,
            IOilService oilService)
        {
            var result = await oilService.DeleteOilAsync(id);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.NotFound(result);
        }
    }
}
