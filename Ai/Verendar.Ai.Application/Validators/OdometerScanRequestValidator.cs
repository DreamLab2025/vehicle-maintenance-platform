using Verendar.Ai.Application.Dtos.OdometerScan;

namespace Verendar.Ai.Application.Validators
{
    public class OdometerScanRequestValidator : AbstractValidator<OdometerScanRequest>
    {
        public OdometerScanRequestValidator()
        {
            RuleFor(x => x.MediaFileId)
                .NotEmpty()
                .WithMessage("MediaFileId không được để trống");
        }
    }
}
