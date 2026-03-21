namespace Verendar.Vehicle.Application.Validators
{
    public class UpdateOdometerRequestValidator : AbstractValidator<UpdateOdometerRequest>
    {
        public UpdateOdometerRequestValidator()
        {
            RuleFor(x => x.CurrentOdometer)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Số km phải lớn hơn hoặc bằng 0");
        }
    }
}
