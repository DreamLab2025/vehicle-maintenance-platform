using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class TypeRequestValidator : AbstractValidator<TypeRequest>
    {
        public TypeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên loại xe không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên loại xe tối đa 100 ký tự");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("URL hình ảnh không được để trống")
                .MaximumLength(500)
                .WithMessage("URL hình ảnh tối đa 500 ký tự");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Mô tả không được để trống")
                .MaximumLength(1000)
                .WithMessage("Mô tả tối đa 1000 ký tự");
        }
    }
}
