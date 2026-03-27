using Verendar.Ai.Application.Dtos.AiPrompt;

namespace Verendar.Ai.Application.Validators;

public class RollbackAiPromptRequestValidator : AbstractValidator<RollbackAiPromptRequest>
{
    public RollbackAiPromptRequestValidator()
    {
        RuleFor(x => x.VersionNumber)
            .GreaterThan(0).WithMessage("VersionNumber phải lớn hơn 0");
    }
}
