using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class AssignBookingRequestValidator : AbstractValidator<AssignBookingRequest>
{
    public AssignBookingRequestValidator()
    {
        RuleFor(x => x.GarageMemberId).NotEmpty();
    }
}
