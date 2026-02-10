using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

namespace Verendar.Ai.Application.Helpers;

public static class PromptGenerator
{
  public static string CreateVehicleMaintenancePrompt(
        VehicleInfoDto vehicleInfo,
        DefaultScheduleDto schedule,
        IEnumerable<QuestionAnswerDto>? answers)
  {
    var today = DateTime.Now.ToString("yyyy-MM-dd");

    var scheduleBlock = $"ITEM: {{ \"Code\": \"{schedule.PartCategoryCode}\", \"Initial_Km\": {schedule.InitialKm}, \"Interval_Km\": {schedule.KmInterval}, \"Interval_Month\": {schedule.MonthsInterval} }}";

    var answerBlock = (answers != null && answers.Any())
        ? string.Join("\n", answers.Where(a => !string.IsNullOrWhiteSpace(a.Value))
                                    .Select(a => $"- User Input ({a.Question}): \"{a.Value}\""))
        : "No user input provided.";

    var vehicleName = $"{vehicleInfo.Brand} {vehicleInfo.Model}".Trim();

    return $@"Nhiệm vụ: Dự đoán mốc bảo dưỡng tiếp theo (predictedNextOdometer, predictedNextDate) từ dữ liệu dưới đây. Trả về JSON đúng format, không null cho predictedNextOdometer/predictedNextDate.

Quy tắc: (1) Có Q&A → bắt buộc suy km/ngày từ câu trả lời rồi tính (confidenceScore 0.75–1.0). (2) Chỉ khi Q&A trống hoặc không suy ra được số km/ngày mới dùng lịch hãng (confidenceScore 0.4–0.6).

---
Today: {today} | Xe: {vehicleName} | ODO: {vehicleInfo.CurrentOdometer} km | Mua: {vehicleInfo.PurchaseDate:yyyy-MM-dd}

Q&A (ưu tiên – suy Last_ODO, Last_Date, chu kỳ từ đây):
{answerBlock}

Lịch hãng (fallback): {scheduleBlock}

---
Cách làm:
- Từ Q&A: trích số km (VD ""15000 km"", ""thay ở 20k"") → Last_ODO; ngày (VD ""6/2024"", ""vừa làm"") → Last_Date; chu kỳ nếu user nói (VD ""mỗi 5k km""). Predicted_Next_ODO = Last_ODO + chu kỳ (từ user hoặc R); Predicted_Next_Date = Last_Date + chu kỳ tháng. reasoning ghi rõ ""dựa trên Q&A"".
- Fallback (chỉ khi Q&A trống/không suy ra gì): C={vehicleInfo.CurrentOdometer}. Nếu C<I: Predicted_Next_ODO=I. Nếu C>=I: Predicted_Next_ODO = I + (Floor((C-I)/R)+1)*R. Predicted_Next_Date = today + MonthsInterval. reasoning: ""thiếu dữ liệu, dùng lịch hãng"".
- needsImmediateAttention: true nếu Predicted_Next_ODO <= ODO hiện tại hoặc Predicted_Next_Date <= today.

Output (JSON only, no markdown):
{{""recommendations"":[{{""partCategoryCode"":""{schedule.PartCategoryCode}"",""lastServiceOdometer"":number|null,""lastServiceDate"":""yyyy-MM-dd""|null,""predictedNextOdometer"":number,""predictedNextDate"":""yyyy-MM-dd"",""confidenceScore"":0.4-1.0,""reasoning"":""string"",""needsImmediateAttention"":bool}}],""warnings"":[]}}";
  }
}