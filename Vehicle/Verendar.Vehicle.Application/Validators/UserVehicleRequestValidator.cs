namespace Verendar.Vehicle.Application.Validators
{
    public class UserVehicleRequestValidator : AbstractValidator<UserVehicleRequest>
    {
        public UserVehicleRequestValidator()
        {
            RuleFor(x => x.VehicleVariantId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn phiên bản xe");

            RuleFor(x => x.LicensePlate)
                .NotEmpty()
                .WithMessage("Biển số xe không được để trống")
                .MaximumLength(20)
                .WithMessage("Biển số xe tối đa 20 ký tự");

            RuleFor(x => x.VinNumber)
                .MaximumLength(17)
                .WithMessage("Số VIN tối đa 17 ký tự")
                .When(x => !string.IsNullOrEmpty(x.VinNumber));

            RuleFor(x => x.CurrentOdometer)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Số km hiện tại phải lớn hơn hoặc bằng 0");
        }
    }
}
