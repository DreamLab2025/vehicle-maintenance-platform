using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class UpdateGarageStatusRequestValidator : AbstractValidator<UpdateGarageStatusRequest>
{
    private static readonly GarageStatus[] AllowedStatuses =
        [GarageStatus.Active, GarageStatus.Rejected, GarageStatus.Suspended];

    public UpdateGarageStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Trạng thái không hợp lệ. Chỉ chấp nhận: Active, Rejected, Suspended.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do từ chối là bắt buộc khi trạng thái là Rejected.")
            .When(x => x.Status == GarageStatus.Rejected);
    }
}

public class UpdateBranchStatusRequestValidator : AbstractValidator<UpdateBranchStatusRequest>
{
    public UpdateBranchStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Trạng thái chi nhánh không hợp lệ. Chỉ chấp nhận: Active, Inactive.");
    }
}

public class GarageRequestValidator : AbstractValidator<GarageRequest>
{
    public GarageRequestValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Tên doanh nghiệp không được để trống")
            .MaximumLength(200).WithMessage("Tên doanh nghiệp tối đa 200 ký tự");

        RuleFor(x => x.ShortName)
            .MaximumLength(100).WithMessage("Tên viết tắt tối đa 100 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ShortName));

        RuleFor(x => x.TaxCode)
            .MaximumLength(20).WithMessage("Mã số thuế tối đa 20 ký tự")
            .Matches(@"^\d{10}(-\d{3})?$")
            .WithMessage("Mã số thuế không đúng định dạng (10 hoặc 13 chữ số, ví dụ: 0123456789 hoặc 0123456789-001)")
            .When(x => !string.IsNullOrEmpty(x.TaxCode));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL tối đa 500 ký tự")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Logo URL không hợp lệ")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));
    }
}
