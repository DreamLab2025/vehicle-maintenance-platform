using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class UpdateBookingMechanicStatusRequestValidator : AbstractValidator<UpdateBookingMechanicStatusRequest>
{
    public UpdateBookingMechanicStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is BookingStatus.InProgress or BookingStatus.Completed)
            .WithMessage("Chỉ được cập nhật sang InProgress hoặc Completed.");
        RuleFor(x => x.CurrentOdometer)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CurrentOdometer.HasValue);
    }
}
