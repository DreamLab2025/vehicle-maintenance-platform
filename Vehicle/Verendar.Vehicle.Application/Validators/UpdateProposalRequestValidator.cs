using FluentValidation;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Validators
{
    public class UpdateProposalRequestValidator : AbstractValidator<UpdateProposalRequest>
    {
        public UpdateProposalRequestValidator()
        {
            When(x => x.OdometerAtService.HasValue, () =>
            {
                RuleFor(x => x.OdometerAtService!.Value)
                    .GreaterThan(0)
                    .WithMessage("Số km phải lớn hơn 0.");
            });

            When(x => x.Notes is not null, () =>
            {
                RuleFor(x => x.Notes)
                    .MaximumLength(1000)
                    .WithMessage("Ghi chú không được vượt quá 1000 ký tự.");
            });

            When(x => x.Items is not null, () =>
            {
                RuleFor(x => x.Items)
                    .NotEmpty()
                    .WithMessage("Danh sách items không được rỗng nếu được cung cấp.");

                RuleForEach(x => x.Items)
                    .ChildRules(item =>
                    {
                        item.RuleFor(i => i.Id)
                            .NotEmpty()
                            .WithMessage("Id của item không được rỗng.");
                    });
            });
        }
    }
}
