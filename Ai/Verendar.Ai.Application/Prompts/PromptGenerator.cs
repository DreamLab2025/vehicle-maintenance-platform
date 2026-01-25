using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

namespace Verendar.Ai.Application.Helpers;

public static class PromptGenerator
{
  public static string CreateVehicleMaintenancePrompt(
      VehicleInfoDto vehicleInfo,
      IEnumerable<DefaultScheduleDto>? schedules,
      IEnumerable<QuestionAnswerDto>? answers)
  {
    var today = DateTime.Now.ToString("yyyy-MM-dd");

    var scheduleBlock = (schedules != null && schedules.Any())
        ? string.Join("\n", schedules.Select(s =>
            $"- {s.PartCategoryCode}: Lần đầu {s.InitialKm}km, sau đó {s.KmInterval}km/{s.MonthsInterval} tháng"))
        : "(Không có lịch chuẩn)";

    var answerBlock = (answers != null && answers.Any())
        ? string.Join("\n", answers.Where(a => !string.IsNullOrWhiteSpace(a.Value))
                                   .Select(a => $"- {a.Question}: {a.Value}"))
        : "(Không có thông tin)";

    var vehicleName = $"{vehicleInfo.Brand} {vehicleInfo.Model}".Trim();

    return $@"
Hôm nay: {today}

THÔNG TIN XE:
- Xe: {vehicleName}
- Số km hiện tại: {vehicleInfo.CurrentOdometer:N0}km
- Ngày mua: {vehicleInfo.PurchaseDate:yyyy-MM-dd}

LỊCH BẢO DƯỠNG CHUẨN:
{scheduleBlock}

THÔNG TIN TỪ NGƯỜI DÙNG:
{answerBlock}

YÊU CẦU TÍNH TOÁN:
1. Xác định số km lần thay cuối (lastServiceOdometer):
   - Nếu người dùng nói ""thay cách đây X km"": lastServiceOdometer = currentOdometer - X
   - Nếu người dùng nói thời gian (VD: ""1 tháng trước""): ước tính km dựa trên thời gian
   - KHÔNG được tự giả định giá trị nếu không có thông tin

2. Xác định ngày thay cuối (lastServiceDate):
   - Nếu người dùng nói thời gian tương đối: tính từ hôm nay ngược lại
   - Ví dụ: ""1 tháng trước"" → {today} trừ 1 tháng

3. Tính lần thay tiếp theo:
   - predictedNextOdometer = lastServiceOdometer + kmInterval
   - predictedNextDate = lastServiceDate + monthsInterval

CHÚ Ý:
- ĐỌC KỸ câu trả lời người dùng để trích xuất số km chính xác
- KHÔNG tự nghĩ ra số liệu khi người dùng không cung cấp
- Nếu thiếu thông tin, đặt giá trị null và ghi vào warnings

Trả về JSON (CHỈ JSON, không text khác):
{{
  ""recommendations"": [
    {{
      ""partCategoryCode"": ""mã_linh_kiện"",
      ""lastServiceOdometer"": số_km_hoặc_null,
      ""lastServiceDate"": ""yyyy-MM-dd""_hoặc_null,
      ""predictedNextOdometer"": số_km_hoặc_null,
      ""predictedNextDate"": ""yyyy-MM-dd""_hoặc_null,
      ""confidenceScore"": 0.0-1.0,
      ""reasoning"": ""giải_thích_ngắn"",
      ""needsImmediateAttention"": true/false
    }}
  ],
  ""warnings"": [""cảnh_báo_nếu_thiếu_thông_tin""]
}}";
  }
}