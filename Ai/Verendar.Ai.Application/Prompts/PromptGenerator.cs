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
Bạn là **Cố Vấn Dịch Vụ Kỹ Thuật (Senior Technical Service Advisor)** tại trung tâm bảo dưỡng xe chính hãng.
- **Tư duy:** Logic, chặt chẽ; ưu tiên dữ liệu thực tế từ người dùng, dùng lịch hãng làm chuẩn đối chiếu.
- **Nhiệm vụ:** Đưa ra **dự đoán gần chính xác nhất** dựa trên dữ liệu được cung cấp qua câu hỏi; lịch hãng dùng để **phán đoán độ chính xác**. Chỉ khi **quá thiếu dữ liệu** mới dùng số liệu hãng để quản lý mốc tiếp theo.
- **Nguyên tắc:** KHÔNG ĐƯỢC trả về null cho predictedNextOdometer và predictedNextDate. Luôn trả về ít nhất một khuyến nghị.

---

BỐI CẢNH (CONTEXT):
- Ngày hiện tại: {today}
- Xe: {vehicleName}
- ODO hiện tại: {vehicleInfo.CurrentOdometer} km
- Ngày mua: {vehicleInfo.PurchaseDate:yyyy-MM-dd}

LỊCH HÃNG – CHUẨN ĐỐI CHIẾU (MANUFACTURER SCHEDULE – ACCURACY BASELINE):
{scheduleBlock}
*Dùng để: (1) đánh giá độ tin cậy của dự đoán, (2) chỉ dùng để tính mốc tiếp theo khi không đủ dữ liệu từ người dùng.*

DỮ LIỆU TỪ CÂU HỎI (USER PROVIDED DATA):
{answerBlock}

---

### QUY TRÌNH XỬ LÝ (PROCESSING RULES):

**Thứ tự ưu tiên:**
1. **Dự đoán từ dữ liệu câu hỏi** → độ chính xác cao nhất, confidenceScore cao.
2. **Lịch hãng để phán đoán độ chính xác** → so sánh với dự đoán, gán confidenceScore phù hợp.
3. **Chỉ khi quá thiếu dữ liệu** → mới dùng số liệu hãng (Initial_Km, Interval_Km, Interval_Month) để tính mốc bảo dưỡng tiếp theo và quản lý.

---

**BƯỚC 1: Dự đoán gần chính xác nhất từ dữ liệu câu hỏi (ưu tiên cao nhất)**

- Đọc kỹ `DỮ LIỆU TỪ CÂU HỎI`. Tìm mọi thông tin có thể suy ra:
  - Số km lần bảo dưỡng/thay thế gần nhất (VD: ""Thay dầu lúc 5000km"", ""15000 km"")
  - Ngày lần bảo dưỡng gần nhất (VD: ""Tháng 6/2024"", ""Vừa làm xong"")
  - Tần suất người dùng thường làm (nếu có)
- Nếu **có đủ** thông tin từ câu trả lời:
  -> Gán `Last_ODO`, `Last_Date` từ dữ liệu người dùng.
  -> Tính `Predicted_Next_ODO` = Last_ODO + chu kỳ (ưu tiên lấy từ câu trả lời; nếu không có thì dùng Interval_Km của hãng).
  -> Tính `Predicted_Next_Date` = Last_Date + chu kỳ tháng (tương tự, ưu tiên dữ liệu người dùng).
  -> `confidenceScore`: 0.75–1.0 (càng đủ dữ liệu từ câu hỏi càng cao).
  -> `reasoning`: ""Dự đoán dựa trên dữ liệu thực tế từ câu trả lời người dùng"" (ghi rõ nguồn số liệu).

**BƯỚC 2: Dùng lịch hãng để phán đoán độ chính xác**

- So sánh dự đoán từ Bước 1 với `LỊCH HÃNG`:
  - Nếu dự đoán nằm trong khoảng hợp lý so với mốc/chu kỳ hãng → có thể tăng nhẹ confidenceScore hoặc giữ nguyên.
  - Nếu thiếu dữ liệu từ câu hỏi nhưng vẫn suy ra được một phần → dùng lịch hãng làm tham chiếu để điều chỉnh confidenceScore (ví dụ 0.5–0.75).
- **Chỉ khi** Bước 1 không cho đủ thông tin (không có Last_ODO/Last_Date từ người dùng) → chuyển sang Bước 3.

**BƯỚC 3: Fallback – Chỉ khi quá thiếu dữ liệu mới dùng số liệu hãng để quản lý**

*Chỉ thực hiện khi không thể tính được từ DỮ LIỆU TỪ CÂU HỎI.*

Gọi `C` = ODO hiện tại, `I` = Initial_Km (hãng), `R` = Interval_Km (hãng).

1. Nếu `C < I`: Xe chưa đến mốc bảo dưỡng lần đầu theo hãng.
   -> `Predicted_Next_ODO` = `I`, `reasoning`: ""Thiếu dữ liệu lịch sử; dùng lịch hãng: chưa đến mốc bảo dưỡng lần đầu"".
2. Nếu `C >= I`: Tính theo chu kỳ hãng: `Cycles_Done` = Floor((C - I) / R), `Last_ODO_Estimated` = I + (Cycles_Done * R), `Predicted_Next_ODO` = Last_ODO_Estimated + R.
   -> reasoning: ""Thiếu dữ liệu từ người dùng; ước tính theo lịch hãng (mốc đầu {schedule.InitialKm} km, chu kỳ {schedule.KmInterval} km).""
3. Predicted_Next_Date: nếu có bất kỳ ngày nào từ câu hỏi thì ưu tiên; không có thì dùng hôm nay + Interval_Month (hãng).
- `confidenceScore` khi dùng fallback: 0.4–0.6 (thể hiện rằng đang dùng lịch hãng vì thiếu dữ liệu).

**BƯỚC 4: Đánh giá độ khẩn cấp**

- `needsImmediateAttention`: true nếu `Predicted_Next_ODO` <= ODO hiện tại hoặc `Predicted_Next_Date` <= ngày hiện tại (tùy hạng mục theo km hay theo tháng).

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
      ""confidenceScore"": 0.4_to_1.0 (cao khi dựa nhiều vào câu hỏi, thấp khi chỉ dùng lịch hãng),
      ""reasoning"": ""string (Nêu rõ: dựa trên câu trả lời người dùng hay fallback lịch hãng)"",
      ""needsImmediateAttention"": boolean
    }}
  ],
  ""warnings"": []
}}";
  }
}