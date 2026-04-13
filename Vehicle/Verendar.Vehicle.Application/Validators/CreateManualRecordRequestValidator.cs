namespace Verendar.Vehicle.Application.Validators
{
    public class CreateManualRecordRequestValidator : AbstractValidator<CreateManualRecordRequest>
    {
        public CreateManualRecordRequestValidator()
        {
            RuleFor(x => x.UserVehicleId)
                .NotEmpty()
                .WithMessage("ID xe không được để trống");

            RuleFor(x => x.GarageName)
                .NotEmpty()
                .WithMessage("Tên ga-ra không được để trống")
                .MaximumLength(200)
                .WithMessage("Tên ga-ra tối đa 200 ký tự");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Cần ít nhất một phụ tùng trong phiếu bảo dưỡng");

            RuleFor(x => x.OdometerAtService)
                .GreaterThanOrEqualTo(0)
                .When(x => x.OdometerAtService.HasValue)
                .WithMessage("Số km bảo dưỡng phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .When(x => x.Notes != null)
                .WithMessage("Ghi chú tối đa 2000 ký tự");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.PartCategorySlug)
                    .NotEmpty()
                    .WithMessage("Slug linh kiện không được để trống")
                    .MaximumLength(50)
                    .WithMessage("Slug linh kiện tối đa 50 ký tự");

                item.RuleFor(x => x.KmCanRun)
                    .GreaterThan(0)
                    .WithMessage("Số km có thể chạy phải lớn hơn 0");

                item.RuleFor(x => x.MonthsCanRun)
                    .GreaterThan(0)
                    .WithMessage("Số tháng sử dụng phải lớn hơn 0");

                item.RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Price.HasValue)
                    .WithMessage("Giá không được âm");

                item.RuleFor(x => x.CustomPartName)
                    .MaximumLength(200)
                    .When(x => x.CustomPartName != null)
                    .WithMessage("Tên phụ tùng tối đa 200 ký tự");
            });
        }
    }
}
