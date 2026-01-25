namespace Verendar.Ai.Application.Dtos.VehicleService;

/// <summary>
/// Lightweight DTO for Vehicle Service API responses - only contains fields needed for AI analysis
/// This reduces payload size and memory usage by ignoring unnecessary fields
/// </summary>
public class VehicleServiceUserVehicleResponse
{
    public int CurrentOdometer { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public VehicleServiceVariantResponse UserVehicleVariant { get; set; } = null!;
}

public class VehicleServiceVariantResponse
{
    public VehicleServiceModelResponse Model { get; set; } = null!;
}

public class VehicleServiceModelResponse
{
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
}

public class VehicleServiceDefaultScheduleResponse
{
    public int InitialKm { get; set; }
    public int KmInterval { get; set; }
    public int MonthsInterval { get; set; }
    public bool RequiresOdometerTracking { get; set; }
    public bool RequiresTimeTracking { get; set; }
}
