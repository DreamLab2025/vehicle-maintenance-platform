namespace Verendar.Ai.Application.Prompts;

public static class OdometerScanPrompt
{
    public const string Instructions =
        """
            Đây là ảnh đồng hồ tốc độ/công-tơ-mét của xe. Hãy đọc số km hiển thị trên màn hình odometer.
            Trả về JSON theo đúng cấu trúc sau (không thêm bất kỳ văn bản nào khác):
            {
              "detectedOdometer": <số nguyên km, hoặc null nếu không đọc được>,
              "confidence": "<high|medium|low>",
              "message": "<giải thích ngắn gọn>"
            }
            Nếu không thể đọc được số km, trả về detectedOdometer = null và giải thích lý do trong message.
            """;
}
