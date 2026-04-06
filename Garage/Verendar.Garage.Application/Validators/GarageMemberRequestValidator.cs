using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class AddMemberRequestValidator : AbstractValidator<AddMemberRequest>
{
    private static readonly MemberRole[] AllowedRoles = [MemberRole.Manager, MemberRole.Mechanic];

    public AddMemberRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(200).WithMessage("Họ tên tối đa 200 ký tự");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(256).WithMessage("Email tối đa 256 ký tự");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự");

        RuleFor(x => x.Role)
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage("Vai trò không hợp lệ. Chỉ chấp nhận: Manager, Mechanic.");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("BranchId không được để trống");

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Mật khẩu tối thiểu 8 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}

public class UpdateMemberStatusRequestValidator : AbstractValidator<UpdateMemberStatusRequest>
{
    public UpdateMemberStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Trạng thái thành viên không hợp lệ.");
    }
}
