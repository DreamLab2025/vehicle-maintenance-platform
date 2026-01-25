using System.Text;
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

    var vehicleName = $"{vehicleInfo.Brand} {vehicleInfo.Model} {(vehicleInfo.Variant ?? "")}".Trim();
    var vehicleType = vehicleInfo.IsUsedVehicle ? "Xe cũ" : "Xe mới";

    return $@"
Hôm nay: {today}

XE:
{vehicleName} - {vehicleType} - {vehicleInfo.CurrentOdometer:N0}km - Mua: {vehicleInfo.PurchaseDate:yyyy-MM-dd}

LỊCH CHUẨN:
{scheduleBlock}

THÔNG TIN TỪ NGƯỜI DÙNG:
{answerBlock}

YÊU CẦU:
Phân tích để ước tính lần thay cuối và lần thay tiếp theo cho linh kiện.
- Ưu tiên: Câu trả lời người dùng > Tính theo km > Lịch chuẩn
- Nếu ""thay gần đây"", tính thời gian tương đối từ hôm nay
- Nếu ""không nhớ"", ước tính dựa trên tổng km và chu kỳ

Trả về JSON:
{{
  ""recommendations"": [
    {{
      ""partCategoryCode"": ""mã_linh_kiện"",
      ""lastServiceOdometer"": số_km,
      ""lastServiceDate"": ""yyyy-MM-dd"",
      ""predictedNextOdometer"": số_km,
      ""predictedNextDate"": ""yyyy-MM-dd"",
      ""confidenceScore"": 0.0-1.0,
      ""reasoning"": ""lý_do_ngắn_gọn"",
      ""needsImmediateAttention"": true/false
    }}
  ],
  ""warnings"": []
}}";
  }
}