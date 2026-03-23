using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class GarageBranchRequestValidator : AbstractValidator<GarageBranchRequest>
{
    public GarageBranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chi nhánh không được để trống")
            .MaximumLength(200).WithMessage("Tên chi nhánh tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description != null);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(500).WithMessage("Cover image URL tối đa 500 ký tự")
            .When(x => x.CoverImageUrl != null);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.TaxCode)
            .MaximumLength(20).WithMessage("Mã số thuế tối đa 20 ký tự")
            .When(x => x.TaxCode != null);

        RuleFor(x => x.Address).NotNull().WithMessage("Địa chỉ không được để trống");

        RuleFor(x => x.Address.ProvinceCode)
            .NotEmpty().WithMessage("Mã tỉnh/thành không được để trống")
            .MaximumLength(10).WithMessage("Mã tỉnh/thành tối đa 10 ký tự")
            .When(x => x.Address != null);

        RuleFor(x => x.Address.WardCode)
            .NotEmpty().WithMessage("Mã phường/xã không được để trống")
            .MaximumLength(20).WithMessage("Mã phường/xã tối đa 20 ký tự")
            .When(x => x.Address != null);

        RuleFor(x => x.Address.StreetDetail)
            .NotEmpty().WithMessage("Địa chỉ chi tiết không được để trống")
            .MaximumLength(500).WithMessage("Địa chỉ chi tiết tối đa 500 ký tự")
            .When(x => x.Address != null);

        RuleFor(x => x.WorkingHours)
            .NotNull().WithMessage("Giờ làm việc không được để trống");

        RuleFor(x => x.WorkingHours.Schedule)
            .NotEmpty().WithMessage("Lịch làm việc không được để trống")
            .When(x => x.WorkingHours != null);
    }
}
