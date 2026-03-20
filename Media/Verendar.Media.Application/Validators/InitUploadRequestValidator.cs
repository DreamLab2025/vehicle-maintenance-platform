namespace Verendar.Media.Application.Validators
{
    public class InitUploadRequestValidator : AbstractValidator<InitUploadRequest>
    {
        public InitUploadRequestValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("Tên file không được để trống.");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("Content type không được để trống.");

            RuleFor(x => x.Size)
                .GreaterThan(0).WithMessage("Kích thước file phải lớn hơn 0.");

            RuleFor(x => x.FileType)
                .IsInEnum().WithMessage("Loại file không hợp lệ.");
        }
    }
}
