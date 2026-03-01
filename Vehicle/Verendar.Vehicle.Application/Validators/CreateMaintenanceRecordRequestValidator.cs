using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class CreateMaintenanceRecordRequestValidator : AbstractValidator<CreateMaintenanceRecordRequest>
    {
        public CreateMaintenanceRecordRequestValidator()
        {
            RuleFor(x => x.OdometerAtService)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Số km bảo dưỡng phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Cần ít nhất một phụ tùng thay thế trong phiếu bảo dưỡng");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.PartCategoryCode)
                    .NotEmpty()
                    .WithMessage("Mã linh kiện không được để trống")
                    .MaximumLength(50)
                    .WithMessage("Mã linh kiện tối đa 50 ký tự");
                item.RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Price.HasValue)
                    .WithMessage("Đơn giá phụ tùng không được âm");
                item.RuleFor(x => x.InstanceIdentifier)
                    .MaximumLength(50)
                    .When(x => x.InstanceIdentifier != null)
                    .WithMessage("Instance identifier tối đa 50 ký tự");
                item.RuleFor(x => x.ItemNotes)
                    .MaximumLength(500)
                    .When(x => x.ItemNotes != null)
                    .WithMessage("Ghi chú dòng phụ tùng tối đa 500 ký tự");
            });

            RuleFor(x => x.TotalCost)
                .GreaterThanOrEqualTo(0)
                .When(x => x.TotalCost.HasValue)
                .WithMessage("Tổng chi phí không được âm");

            RuleFor(x => x.GarageName)
                .MaximumLength(200)
                .When(x => x.GarageName != null)
                .WithMessage("Tên ga-ra tối đa 200 ký tự");

            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .When(x => x.Notes != null)
                .WithMessage("Ghi chú phiếu tối đa 2000 ký tự");

            RuleFor(x => x.InvoiceImageUrl)
                .MaximumLength(500)
                .When(x => x.InvoiceImageUrl != null)
                .WithMessage("URL ảnh hóa đơn tối đa 500 ký tự");
        }
    }
}
