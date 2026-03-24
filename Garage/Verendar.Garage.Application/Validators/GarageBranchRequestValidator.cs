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
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Cover image URL không hợp lệ")
            .When(x => x.CoverImageUrl != null);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự")
            .Matches(@"^(0|\+84)[3-9][0-9]{8}$").WithMessage("Số điện thoại không đúng định dạng (ví dụ: 0901234567 hoặc +84901234567)")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.TaxCode)
            .MaximumLength(20).WithMessage("Mã số thuế tối đa 20 ký tự")
            .Matches(@"^\d{10}(-\d{3})?$")
            .WithMessage("Mã số thuế không đúng định dạng (10 hoặc 13 chữ số)")
            .When(x => x.TaxCode != null);

        // ── Address ──────────────────────────────────────────────────────────

        RuleFor(x => x.Address).NotNull().WithMessage("Địa chỉ không được để trống");

        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address.ProvinceCode)
                .NotEmpty().WithMessage("Mã tỉnh/thành không được để trống")
                .MaximumLength(10).WithMessage("Mã tỉnh/thành tối đa 10 ký tự");

            RuleFor(x => x.Address.WardCode)
                .NotEmpty().WithMessage("Mã phường/xã không được để trống")
                .MaximumLength(20).WithMessage("Mã phường/xã tối đa 20 ký tự");

            RuleFor(x => x.Address.HouseNumber)
                .MaximumLength(20).WithMessage("Số nhà tối đa 20 ký tự")
                .When(x => x.Address.HouseNumber != null);

            RuleFor(x => x.Address.StreetDetail)
                .NotEmpty().WithMessage("Địa chỉ chi tiết không được để trống")
                .MaximumLength(500).WithMessage("Địa chỉ chi tiết tối đa 500 ký tự");
        });

        // ── WorkingHours ──────────────────────────────────────────────────────

        RuleFor(x => x.WorkingHours)
            .NotNull().WithMessage("Giờ làm việc không được để trống");

        When(x => x.WorkingHours != null, () =>
        {
            RuleFor(x => x.WorkingHours.Schedule)
                .NotEmpty().WithMessage("Lịch làm việc không được để trống");

            RuleFor(x => x.WorkingHours.Schedule)
                .Must(schedule => schedule == null || schedule.Values.All(
                    day => day.IsClosed || day.OpenTime < day.CloseTime))
                .WithMessage("Giờ mở cửa phải trước giờ đóng cửa")
                .When(x => x.WorkingHours.Schedule != null);
        });
    }
}
