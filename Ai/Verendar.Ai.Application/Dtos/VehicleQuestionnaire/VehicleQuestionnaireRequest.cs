namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

public class VehicleQuestionnaireRequest
{
    public Guid UserId { get; set; }

    public Guid UserVehicleId { get; set; }

    public VehicleInfoDto VehicleInfo { get; set; } = null!;

    public List<DefaultScheduleDto> DefaultSchedules { get; set; } = new();

    public List<QuestionAnswerDto> Answers { get; set; } = new();
}

public class VehicleInfoDto
{
    public string Type { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int CurrentOdometer { get; set; }
    public DateTime PurchaseDate { get; set; }
}


public class DefaultScheduleDto
{
    public string PartCategoryCode { get; set; } = string.Empty;

    public string PartCategoryName { get; set; } = string.Empty;

    public int InitialKm { get; set; }

    public int KmInterval { get; set; }

    public int MonthsInterval { get; set; }
}


public class QuestionAnswerDto
{
    public string Question { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
