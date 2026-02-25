using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

namespace Verendar.Ai.Application.Helpers
{
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

        return $@"Nhiệm vụ: Dự đoán mốc bảo dưỡng tiếp theo và trả về JSON đúng format. Điền lastServiceOdometer/lastServiceDate khi suy được từ Q&A; predictedNextOdometer/predictedNextDate luôn có giá trị (không null).

Quy tắc ưu tiên: (1) Có Q&A → suy L (last service), R_km (chu kỳ km), R_month (chu kỳ tháng) từ câu trả lời; confidenceScore 0.75–1.0. (2) Chỉ khi Q&A trống hoặc không suy được mới dùng lịch hãng (Initial_Km I, Interval_Km R, Interval_Month); confidenceScore 0.4–0.6.

---
Today: {today} | Xe: {vehicleName} | Current_ODO (C): {vehicleInfo.CurrentOdometer} km | Mua: {vehicleInfo.PurchaseDate:yyyy-MM-dd}

Q&A (ưu tiên):
    {answerBlock}

Lịch hãng (fallback): {scheduleBlock}

---
Cách làm (tổng quát, áp dụng mọi trường hợp):

1) Suy L (lastServiceOdometer / lastServiceDate) từ Q&A:
   - ""X km trước"" / ""X km ago"" / ""over X km ago"" → L_ODO = C - X. Ghi lastServiceOdometer = L_ODO.
   - ""Thay ở Y km"" / ""at Y km"" → L_ODO = Y.
   - ""Z tháng/ngày/tuần trước"" → L_Date = today trừ Z. Ghi lastServiceDate = L_Date (yyyy-MM-dd).
   - Nếu không suy được từ Q&A → lastServiceOdometer/lastServiceDate = null.

2) Suy chu kỳ từ Q&A:
   - R_km: từ câu kiểu ""mỗi A km"", ""A–B km"" (lấy giá trị hoặc biên trên). Không có thì dùng lịch hãng Interval_Km.
   - R_month: từ câu kiểu ""mỗi A tháng"". Không có thì dùng lịch hãng Interval_Month.

3) Predicted_Next_ODO (predictedNextOdometer):
   - Ràng buộc bắt buộc: predictedNextOdometer PHẢI >= C (số km hiện tại). Đây là mốc km mà người dùng NÊN thay/bảo dưỡng — không thể là mốc đã qua (ví dụ C=8000 thì không được trả về 6500; phải là 8000 trở lên).
   - Khi có L_ODO và R_km: dãy mốc là L_ODO + R_km, L_ODO + 2*R_km, ... Chọn mốc ĐẦU TIÊN trong dãy mà >= C. Nếu mốc tính được (vd 6500) < C (8000) thì bỏ qua, lấy mốc tiếp theo (8000). Cấm dùng C + R_km. Cấm trả về giá trị < C.
   - Công thức: next = L_ODO + ceil((C - L_ODO) / R_km) * R_km, với ceil làm tròn lên (đảm bảo next >= C). Nếu C <= L_ODO thì next = L_ODO + R_km.
   - Fallback (không có L_ODO): dùng I, R từ lịch hãng. Nếu C < I thì next = I. Nếu C >= I thì next = I + (floor((C - I) / R) + 1) * R.

4) Predicted_Next_Date (predictedNextDate):
   - Nếu predictedNextOdometer <= C hoặc predictedNextDate theo thời gian đã <= today → đã quá hạn: predictedNextDate = today.
   - Nếu chưa quá hạn và có L_Date, R_month: predictedNextDate = L_Date + R_month (theo tháng).
   - Fallback: today + Interval_Month (lịch hãng).

5) needsImmediateAttention: true khi và chỉ khi predictedNextOdometer <= C hoặc predictedNextDate <= today.

6) reasoning: Giải thích ngắn bằng tiếng Việt: nguồn L_ODO/L_Date, R_km/R_month, cách tính mốc tiếp theo, và (nếu quá hạn) vì sao cần bảo dưỡng ngay.

7) warnings: Mảng chuỗi. Thêm cảnh báo khi xe đã vượt mốc bảo dưỡng dự kiến (C > mốc kế sau L_ODO): nội dung nêu rõ mốc đã vượt (km) và yêu cầu bảo dưỡng ngay tại C km hiện tại. Không có thì [].

Kiểm tra trước khi trả về: predictedNextOdometer >= C (nếu không thì tính lại; giá trị < C là sai).

Output (JSON only, no markdown):
{{""recommendations"":[{{""partCategoryCode"":""{schedule.PartCategoryCode}"",""lastServiceOdometer"":number|null,""lastServiceDate"":""yyyy-MM-dd""|null,""predictedNextOdometer"":number,""predictedNextDate"":""yyyy-MM-dd"",""confidenceScore"":0.4-1.0,""reasoning"":""string"",""needsImmediateAttention"":bool}}],""warnings"":[""string""]}}";
      }
    }
}