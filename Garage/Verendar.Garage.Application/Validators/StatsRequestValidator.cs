using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class StatsRequestValidator : AbstractValidator<StatsRequest>
{
    private static readonly string[] ValidPeriods = ["weekly", "monthly", "quarterly"];

    public StatsRequestValidator()
    {
        When(r => r.From.HasValue, () =>
        {
            RuleFor(r => r.From!.Value)
                .Must(from => from >= DateOnly.FromDateTime(DateTime.Today.AddYears(-3)))
                .WithMessage("Chỉ được truy vấn dữ liệu trong 3 năm gần nhất.");
        });

        When(r => r.From.HasValue && r.To.HasValue, () =>
        {
            RuleFor(r => r)
                .Must(r => r.From!.Value <= r.To!.Value)
                .WithMessage("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        });

        When(r => !string.IsNullOrEmpty(r.Period), () =>
        {
            RuleFor(r => r.Period!)
                .Must(p => ValidPeriods.Contains(p.ToLowerInvariant()))
                .WithMessage("Period phải là 'weekly', 'monthly' hoặc 'quarterly'.");
        });
    }
}
