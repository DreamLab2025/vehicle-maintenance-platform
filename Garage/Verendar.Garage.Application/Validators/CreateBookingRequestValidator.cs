using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.GarageBranchId)
            .NotEmpty().WithMessage("Chi nhánh không được để trống");

        RuleFor(x => x.UserVehicleId)
            .NotEmpty().WithMessage("Xe không được để trống");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Thời gian đặt lịch không được để trống");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Booking phải có ít nhất một mục");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i)
                    .Must(i => (i.ProductId.HasValue ? 1 : 0) + (i.ServiceId.HasValue ? 1 : 0) + (i.BundleId.HasValue ? 1 : 0) == 1)
                    .WithMessage("Mỗi mục booking phải chỉ định đúng một trong ProductId, ServiceId hoặc BundleId.");
            });
    }
}
