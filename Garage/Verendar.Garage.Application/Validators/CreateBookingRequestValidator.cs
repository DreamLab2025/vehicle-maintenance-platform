using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.GarageBranchId).NotEmpty();
        RuleFor(x => x.GarageProductId).NotEmpty();
        RuleFor(x => x.UserVehicleId).NotEmpty();
        RuleFor(x => x.ScheduledAt).NotEqual(default(DateTime));
        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .When(x => x.Note != null);
    }
}
