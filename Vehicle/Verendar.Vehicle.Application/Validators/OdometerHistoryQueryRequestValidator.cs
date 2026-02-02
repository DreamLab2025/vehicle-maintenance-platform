using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class OdometerHistoryQueryRequestValidator : AbstractValidator<OdometerHistoryQueryRequest>
    {
        public OdometerHistoryQueryRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Số trang phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Kích thước trang phải từ 1 đến 100");

            RuleFor(x => x)
                .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
                .WithMessage("Từ ngày phải nhỏ hơn hoặc bằng đến ngày")
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
        }
    }
}
