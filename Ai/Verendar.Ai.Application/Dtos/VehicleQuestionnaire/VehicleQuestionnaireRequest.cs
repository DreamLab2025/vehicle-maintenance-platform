namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire
{
    public class VehicleQuestionnaireRequest
    {
        public Guid UserVehicleId { get; set; }

        public Guid VehicleModelId { get; set; }

        public string PartCategoryCode { get; set; } = string.Empty;

        public List<QuestionAnswerDto> Answers { get; set; } = new();
    }

    public class VehicleInfoDto
    {
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

        public bool RequiresOdometerTracking { get; set; }

        public bool RequiresTimeTracking { get; set; }
    }


    public class QuestionAnswerDto
    {
        public string Question { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
