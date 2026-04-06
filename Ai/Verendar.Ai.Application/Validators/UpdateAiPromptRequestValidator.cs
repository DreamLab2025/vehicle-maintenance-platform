using Verendar.Ai.Application.Dtos.AiPrompt;

namespace Verendar.Ai.Application.Validators;

public class UpdateAiPromptRequestValidator : AbstractValidator<UpdateAiPromptRequest>
{
    public UpdateAiPromptRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content không được để trống")
            .MinimumLength(10).WithMessage("Content phải có ít nhất 10 ký tự");

        RuleFor(x => x.Provider)
            .Must(v => Enum.IsDefined(typeof(AiProvider), v))
            .WithMessage("Provider không hợp lệ");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name không được vượt quá 100 ký tự")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description không được vượt quá 500 ký tự")
            .When(x => x.Description != null);
    }
}
