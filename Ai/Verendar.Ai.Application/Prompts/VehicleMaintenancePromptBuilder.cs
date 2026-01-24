using Verendar.Ai.Application.Dtos.VehicleQuestionnaire; // Namespace chứa DTO của bạn

namespace Verendar.Ai.Application.Prompts;

public class VehicleMaintenancePromptBuilder
{
    private readonly VehicleInfoDto _vehicleInfo;
    private readonly List<DefaultScheduleDto> _schedules = new();
    private readonly List<QuestionAnswerDto> _answers = new();

    public VehicleMaintenancePromptBuilder(VehicleInfoDto vehicleInfo)
    {
        _vehicleInfo = vehicleInfo;
    }

    public VehicleMaintenancePromptBuilder AddSchedules(IEnumerable<DefaultScheduleDto> schedules)
    {
        if (schedules != null)
        {
            _schedules.AddRange(schedules);
        }
        return this;
    }

    public VehicleMaintenancePromptBuilder AddAnswers(IEnumerable<QuestionAnswerDto> answers)
    {
        if (answers != null)
        {
            _answers.AddRange(answers.Where(a => !string.IsNullOrWhiteSpace(a.Value)));
        }
        return this;
    }

    public string Build()
    {
        var scheduleBlock = _schedules.Any()
            ? string.Join(Environment.NewLine, _schedules.Select(s =>
                $"- {s.PartCategoryCode}: {s.PartCategoryName} (First: {s.InitialKm}km, Then every: {s.KmInterval}km or {s.MonthsInterval} months)"))
            : "(No standard schedule provided)";

        var feedbackBlock = _answers.Any()
            ? string.Join(Environment.NewLine, _answers.Select(a => $"Q: {a.Question} -> A: {a.Value}"))
            : "(No user feedback provided, rely entirely on Odometer and Standard Schedule)";

        var vehicleType = _vehicleInfo.IsUsedVehicle ? "Used (Xe cũ)" : "New (Xe mới)";
        var vehicleName = $"{_vehicleInfo.Brand} {_vehicleInfo.Model} {(_vehicleInfo.Variant ?? "")}".Trim();

        return $@"You are an expert Vehicle Maintenance AI assisting a Vietnamese user.
Task: Analyze Vehicle Info, Standard Schedule, and User Feedback to estimate the maintenance status for specific parts.

--- DATA INPUT ---

[VEHICLE PROFILE]
Model: {vehicleName}
Odometer: {_vehicleInfo.CurrentOdometer:N0} km
Purchase Date: {_vehicleInfo.PurchaseDate:yyyy-MM-dd}
Type: {vehicleType}

[STANDARD MAINTENANCE SCHEDULE]
{scheduleBlock}

[USER FEEDBACK]
{feedbackBlock}

--- LOGIC RULES ---
1. **Last Service Calculation**:
   - Priority 1: Calculate backwards from User Feedback (e.g., ""changed 3 months ago"" or ""ran 2000km since last change"").
   - Priority 2 (if feedback missing): 
     - If New Car: Last Service = Purchase Date (0 km).
     - If Used Car: Estimate based on current odometer modulo interval.

2. **Next Service Calculation**:
   - Next Date = Last Service Date + Months Interval.
   - Next Odometer = Last Service Odometer + Km Interval.
   - Select whichever comes FIRST (Time or Distance).

3. **Output Requirements**:
   - Return STRICT JSON only. No markdown formatting.
   - Confidence Score: 1.0 (Confirmed by user), 0.5 (Inferred/Estimated), 0.1 (Guess).
   - Reasoning & Warnings must be in **Vietnamese**.

--- JSON OUTPUT FORMAT ---
{{
  ""recommendations"": [
    {{
      ""partCategoryCode"": ""string (must match PartCategoryCode from Schedule)"",
      ""lastServiceOdometer"": int,
      ""lastServiceDate"": ""yyyy-MM-dd"",
      ""predictedNextOdometer"": int,
      ""predictedNextDate"": ""yyyy-MM-dd"",
      ""confidenceScore"": float (0.0-1.0),
      ""reasoning"": ""string (Giải thích ngắn gọn tiếng Việt cách tính)"",
      ""needsImmediateAttention"": bool
    }}
  ],
  ""warnings"": [
    ""string (Cảnh báo tiếng Việt nếu xe quá hạn bảo dưỡng hoặc có dấu hiệu bất thường từ feedback)""
  ]
}}";
    }
}