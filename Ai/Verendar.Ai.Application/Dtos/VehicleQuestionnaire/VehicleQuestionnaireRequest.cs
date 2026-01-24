namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

/// <summary>
/// Request for AI to analyze vehicle questionnaire and recommend maintenance tracking
/// </summary>
public class VehicleQuestionnaireRequest
{
    /// <summary>
    /// User ID making the request
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User vehicle ID
    /// </summary>
    public Guid UserVehicleId { get; set; }

    /// <summary>
    /// Vehicle information
    /// </summary>
    public VehicleInfoDto VehicleInfo { get; set; } = null!;

    /// <summary>
    /// Default maintenance schedules from manufacturer
    /// </summary>
    public List<DefaultScheduleDto> DefaultSchedules { get; set; } = new();

    /// <summary>
    /// User answers to questionnaire
    /// </summary>
    public List<QuestionAnswerDto> Answers { get; set; } = new();
}

/// <summary>
/// Vehicle information for AI analysis
/// </summary>
public class VehicleInfoDto
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Variant { get; set; }
    public int CurrentOdometer { get; set; }
    public DateTime PurchaseDate { get; set; }
    public bool IsUsedVehicle { get; set; }
}

/// <summary>
/// Default maintenance schedule from manufacturer
/// </summary>
public class DefaultScheduleDto
{
    /// <summary>
    /// Part category code (e.g., "engine_oil", "oil_filter")
    /// </summary>
    public string PartCategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Part category name (e.g., "Dầu động cơ")
    /// </summary>
    public string PartCategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Initial service at this odometer (km)
    /// </summary>
    public int InitialKm { get; set; }

    /// <summary>
    /// Service interval in kilometers
    /// </summary>
    public int KmInterval { get; set; }

    /// <summary>
    /// Service interval in months
    /// </summary>
    public int MonthsInterval { get; set; }
}

/// <summary>
/// Question and answer pair
/// </summary>
public class QuestionAnswerDto
{
    public string Question { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
