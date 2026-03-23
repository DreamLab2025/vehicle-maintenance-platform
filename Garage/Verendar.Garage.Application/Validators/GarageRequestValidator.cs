using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class GarageRequestValidator : AbstractValidator<GarageRequest>
{
    public GarageRequestValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .WithMessage("Tên doanh nghiệp không được để trống")
            .MaximumLength(200)
            .WithMessage("Tên doanh nghiệp tối đa 200 ký tự");

        RuleFor(x => x.ShortName)
            .MaximumLength(100)
            .WithMessage("Tên viết tắt tối đa 100 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ShortName));

        RuleFor(x => x.TaxCode)
            .MaximumLength(20)
            .WithMessage("Mã số thuế tối đa 20 ký tự")
            .When(x => !string.IsNullOrEmpty(x.TaxCode));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .WithMessage("Logo URL tối đa 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));
    }
}
