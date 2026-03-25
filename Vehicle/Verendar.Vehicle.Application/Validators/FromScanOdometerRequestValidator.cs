namespace Verendar.Vehicle.Application.Validators
{
    public class FromScanOdometerRequestValidator : AbstractValidator<FromScanOdometerRequest>
    {
        public FromScanOdometerRequestValidator()
        {
            RuleFor(x => x.MediaFileId)
                .NotEmpty()
                .WithMessage("MediaFileId không được để trống");

            RuleFor(x => x.ConfirmedOdometer)
                .GreaterThan(0)
                .WithMessage("Số km xác nhận phải lớn hơn 0");
        }
    }
}
