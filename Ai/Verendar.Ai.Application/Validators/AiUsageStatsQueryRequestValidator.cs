namespace Verendar.Ai.Application.Validators;

public class AiUsageStatsQueryRequestValidator : AbstractValidator<AiUsageStatsQueryRequest>
{
    public AiUsageStatsQueryRequestValidator()
    {
        RuleFor(x => x.ModelSearch)
            .MaximumLength(100)
            .WithMessage("Tên model tìm kiếm không được vượt quá 100 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.ModelSearch));

        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc <= x.ToUtc)
            .WithMessage("Khoảng thời gian không hợp lệ: FromUtc phải nhỏ hơn hoặc bằng ToUtc");
    }
}
