using Microsoft.AspNetCore.Mvc;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Application.Validators;

namespace Verendar.Vehicle.Apis
{
    public static class MaintenanceProposalApis
    {
        public static IEndpointRouteBuilder MapMaintenanceProposalApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/user-vehicles")
                .MapMaintenanceProposalRoutes()
                .WithTags("Maintenance Proposal Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapMaintenanceProposalRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/{vehicleId:guid}/maintenance-proposals", GetPending)
                .WithName("GetPendingMaintenanceProposals")
                .WithOpenApi(op => { op.Summary = "Lấy danh sách đề xuất bảo dưỡng chờ xác nhận"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<MaintenanceProposalResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

            group.MapPatch("/{vehicleId:guid}/maintenance-proposals/{proposalId:guid}", Update)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateProposalRequest>())
                .WithName("UpdateMaintenanceProposal")
                .WithOpenApi(op => { op.Summary = "Chỉnh sửa đề xuất bảo dưỡng trước khi xác nhận"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<MaintenanceProposalResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

            group.MapPost("/{vehicleId:guid}/maintenance-proposals/{proposalId:guid}/apply", Apply)
                .WithName("ApplyMaintenanceProposal")
                .WithOpenApi(op => { op.Summary = "Xác nhận áp dụng đề xuất bảo dưỡng"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<ApplyProposalResult>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> GetPending(
            [FromRoute] Guid vehicleId,
            [AsParameters] PaginationRequest pagination,
            ICurrentUserService currentUserService,
            IMaintenanceProposalService proposalService,
            CancellationToken ct)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Results.Unauthorized();

            var result = await proposalService.GetPendingByVehicleAsync(userId, vehicleId, pagination, ct);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Update(
            [FromRoute] Guid vehicleId,
            [FromRoute] Guid proposalId,
            [FromBody] UpdateProposalRequest request,
            ICurrentUserService currentUserService,
            IMaintenanceProposalService proposalService,
            CancellationToken ct)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Results.Unauthorized();

            var result = await proposalService.UpdateAsync(userId, vehicleId, proposalId, request, ct);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Apply(
            [FromRoute] Guid vehicleId,
            [FromRoute] Guid proposalId,
            ICurrentUserService currentUserService,
            IMaintenanceProposalService proposalService,
            CancellationToken ct)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Results.Unauthorized();

            var result = await proposalService.ApplyAsync(userId, vehicleId, proposalId, ct);
            return result.ToHttpResult();
        }
    }
}
