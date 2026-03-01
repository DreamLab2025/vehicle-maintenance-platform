using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class BrandRequestValidator : AbstractValidator<BrandRequest>
    {
        public BrandRequestValidator()
        {
            RuleFor(x => x.VehicleTypeId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn loại xe");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên thương hiệu không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên thương hiệu tối đa 100 ký tự");
        }
    }
}
