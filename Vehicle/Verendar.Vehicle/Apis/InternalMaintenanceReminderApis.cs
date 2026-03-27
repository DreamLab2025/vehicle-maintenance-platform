using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Vehicle.Apis;

public static class InternalMaintenanceReminderApis
{
    public static IEndpointRouteBuilder MapInternalMaintenanceReminderApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/internal/maintenance-reminders")
            .WithTags("Internal Maintenance Reminder")
            .RequireAuthorization()
            .MapPost("/lookup", LookupAsync);

        return builder;
    }

    private static async Task<IResult> LookupAsync(
        IMaintenanceReminderLookupService service,
        MaintenanceReminderLookupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.LookupForNotificationAsync(request, cancellationToken);
        return result.ToHttpResult();
    }
}
