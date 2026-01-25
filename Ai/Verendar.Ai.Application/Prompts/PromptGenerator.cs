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

    // Format dữ liệu Schedule
    var scheduleBlock = $"ITEM: {{ \"Code\": \"{schedule.PartCategoryCode}\", \"Initial_Km\": {schedule.InitialKm}, \"Interval_Km\": {schedule.KmInterval}, \"Interval_Month\": {schedule.MonthsInterval} }}";

    // Format dữ liệu User Answer
    var answerBlock = (answers != null && answers.Any())
        ? string.Join("\n", answers.Where(a => !string.IsNullOrWhiteSpace(a.Value))
                                    .Select(a => $"- User Input ({a.Question}): \"{a.Value}\""))
        : "No user input provided.";

    var vehicleName = $"{vehicleInfo.Brand} {vehicleInfo.Model}".Trim();

    return $@"
VAI TRÒ CỦA BẠN (ROLE):
Bạn là một **Cố Vấn Dịch Vụ Kỹ Thuật (Senior Technical Service Advisor)** tại trung tâm bảo dưỡng xe chính hãng.
- **Tư duy:** Logic, chặt chẽ, dựa trên số liệu kỹ thuật, không suy đoán cảm tính.
- **Nhiệm vụ:** Phân tích ODO hiện tại và Lịch Sử Xe để lập kế hoạch bảo dưỡng tiếp theo chính xác tuyệt đối.
- **Nguyên tắc vàng:** Nếu thiếu dữ liệu lịch sử, bạn PHẢI sử dụng công thức toán học để tính toán mốc bảo dưỡng định kỳ dựa trên ODO hiện tại (Fallback Calculation). KHÔNG ĐƯỢC trả về null.

---

BỐI CẢNH (CONTEXT):
- Ngày hiện tại: {today}
- Xe: {vehicleName}
- ODO hiện tại: {vehicleInfo.CurrentOdometer} km
- Ngày mua: {vehicleInfo.PurchaseDate:yyyy-MM-dd}

DỮ LIỆU KỸ THUẬT (STANDARD SCHEDULES):
{scheduleBlock}

DỮ LIỆU NGƯỜI DÙNG (USER HISTORY):
{answerBlock}

---

### QUY TRÌNH XỬ LÝ (PROCESSING RULES):

Với mỗi hạng mục trong `STANDARD SCHEDULES`, hãy thực hiện các bước sau:

**BƯỚC 1: Phân tích Lịch Sử (Real Data Analysis)**
- Kiểm tra `USER HISTORY`. Nếu người dùng xác nhận đã thay thế/bảo dưỡng (VD: ""Thay dầu lúc 5000km"", ""Vừa làm xong""):
  -> `Last_ODO` = Giá trị thực tế.
  -> `Last_Date` = Ngày thực tế.
  -> `Reasoning`: ""Dựa trên lịch sử bảo dưỡng thực tế"".

**BƯỚC 2: Tính toán Kỹ thuật (Technical Fallback)**
*Chỉ chạy bước này nếu Bước 1 không có dữ liệu. Hệ thống giả định xe tuân thủ lịch chuẩn.*

Gọi `C` = ODO Hiện tại, `I` = Mốc Đầu (Initial), `R` = Chu kỳ (Interval).

1. **Kiểm tra Mốc Đầu (Initial Check):**
   - Nếu `C < I`: Xe chưa đến hạn bảo dưỡng lần đầu.
     -> `Predicted_Next_ODO` = `I`.
     -> `Reasoning`: ""Xe mới, chưa đến mốc bảo dưỡng lần đầu"".

2. **Tính toán Chu kỳ (Interval Calculation):**
   - Nếu `C >= I`: Xe đã qua mốc đầu. Tính số chu kỳ định kỳ đã hoàn thành.
   - Công thức: `Cycles_Done` = Math.Floor((C - I) / R).
   - `Last_ODO_Estimated` = `I` + (`Cycles_Done` * `R`).
   - `Predicted_Next_ODO` = `Last_ODO_Estimated` + `R`.
   - `Reasoning`: ""Ước tính kỹ thuật: Đã qua mốc đầu {schedule.InitialKm}km và {Math.Floor((double)(vehicleInfo.CurrentOdometer - schedule.InitialKm) / schedule.KmInterval)} chu kỳ định kỳ"".

3. **Tính Ngày (Date Projection):**
   - `Predicted_Next_Date` = `Last_Date` (nếu có) + `Interval_Month`.
   - Nếu không có `Last_Date`: `Predicted_Next_Date` = Hôm nay + `Interval_Month` (Khuyến nghị thực hiện ngay chu kỳ tới).

**BƯỚC 3: Đánh giá Độ Khẩn Cấp (Urgency)**
- `needsImmediateAttention`: True nếu (`Predicted_Next_ODO` <= `C`).

---

### OUTPUT FORMAT (JSON ONLY - NO MARKDOWN):
{{
  ""recommendations"": [
    {{
      ""partCategoryCode"": ""string"",
      ""lastServiceOdometer"": number_or_null,
      ""lastServiceDate"": ""yyyy-MM-dd""_or_null,
      ""predictedNextOdometer"": number (REQUIRED - DO NOT RETURN NULL),
      ""predictedNextDate"": ""yyyy-MM-dd"" (REQUIRED - DO NOT RETURN NULL),
      ""confidenceScore"": 0.5_to_1.0,
      ""reasoning"": ""string (Technical explanation)"",
      ""needsImmediateAttention"": boolean
    }}
  ],
  ""warnings"": []
}}";
  }
}