using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Validators;

public class CreateGarageProductRequestValidator : AbstractValidator<CreateGarageProductRequest>
{
    public CreateGarageProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(200).WithMessage("Tên sản phẩm tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.MaterialPrice)
            .NotNull().WithMessage("Giá sản phẩm không được để trống");

        RuleFor(x => x.MaterialPrice.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm")
            .When(x => x.MaterialPrice is not null);

        RuleFor(x => x.MaterialPrice.Currency)
            .NotEmpty().MaximumLength(10).WithMessage("Đơn vị tiền tệ không hợp lệ")
            .When(x => x.MaterialPrice is not null);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Thời gian ước tính phải lớn hơn 0")
            .When(x => x.EstimatedDurationMinutes.HasValue);

        RuleFor(x => x.ManufacturerKmInterval)
            .GreaterThan(0).WithMessage("Chu kỳ km phải lớn hơn 0")
            .When(x => x.ManufacturerKmInterval.HasValue);

        RuleFor(x => x.ManufacturerMonthInterval)
            .GreaterThan(0).WithMessage("Chu kỳ tháng phải lớn hơn 0")
            .When(x => x.ManufacturerMonthInterval.HasValue);
    }
}

public class UpdateGarageProductRequestValidator : AbstractValidator<UpdateGarageProductRequest>
{
    public UpdateGarageProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(200).WithMessage("Tên sản phẩm tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.MaterialPrice)
            .NotNull().WithMessage("Giá sản phẩm không được để trống");

        RuleFor(x => x.MaterialPrice.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm")
            .When(x => x.MaterialPrice is not null);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Thời gian ước tính phải lớn hơn 0")
            .When(x => x.EstimatedDurationMinutes.HasValue);

        RuleFor(x => x.ManufacturerKmInterval)
            .GreaterThan(0).WithMessage("Chu kỳ km phải lớn hơn 0")
            .When(x => x.ManufacturerKmInterval.HasValue);

        RuleFor(x => x.ManufacturerMonthInterval)
            .GreaterThan(0).WithMessage("Chu kỳ tháng phải lớn hơn 0")
            .When(x => x.ManufacturerMonthInterval.HasValue);
    }
}

public class UpdateGarageProductStatusRequestValidator : AbstractValidator<UpdateGarageProductStatusRequest>
{
    public UpdateGarageProductStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái sản phẩm không hợp lệ");
    }
}

public class CreateServiceCategoryRequestValidator : AbstractValidator<CreateServiceCategoryRequest>
{
    public CreateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug không được để trống")
            .MaximumLength(50).WithMessage("Slug tối đa 50 ký tự")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ chứa chữ thường, số và dấu gạch ngang");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả tối đa 500 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.IconUrl)
            .MaximumLength(255).WithMessage("Icon URL tối đa 255 ký tự")
            .When(x => x.IconUrl is not null);
    }
}

public class UpdateServiceCategoryRequestValidator : AbstractValidator<UpdateServiceCategoryRequest>
{
    public UpdateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả tối đa 500 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.IconUrl)
            .MaximumLength(255).WithMessage("Icon URL tối đa 255 ký tự")
            .When(x => x.IconUrl is not null);
    }
}

public class CreateGarageServiceRequestValidator : AbstractValidator<CreateGarageServiceRequest>
{
    public CreateGarageServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên dịch vụ không được để trống")
            .MaximumLength(200).WithMessage("Tên dịch vụ tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.LaborPrice)
            .NotNull().WithMessage("Giá công không được để trống");

        RuleFor(x => x.LaborPrice.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Giá công không được âm")
            .When(x => x.LaborPrice is not null);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Thời gian ước tính phải lớn hơn 0")
            .When(x => x.EstimatedDurationMinutes.HasValue);
    }
}

public class UpdateGarageServiceRequestValidator : AbstractValidator<UpdateGarageServiceRequest>
{
    public UpdateGarageServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên dịch vụ không được để trống")
            .MaximumLength(200).WithMessage("Tên dịch vụ tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.LaborPrice)
            .NotNull().WithMessage("Giá công không được để trống");

        RuleFor(x => x.LaborPrice.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Giá công không được âm")
            .When(x => x.LaborPrice is not null);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Thời gian ước tính phải lớn hơn 0")
            .When(x => x.EstimatedDurationMinutes.HasValue);
    }
}

public class UpdateGarageServiceStatusRequestValidator : AbstractValidator<UpdateGarageServiceStatusRequest>
{
    public UpdateGarageServiceStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái dịch vụ không hợp lệ");
    }
}

public class CreateGarageBundleRequestValidator : AbstractValidator<CreateGarageBundleRequest>
{
    public CreateGarageBundleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên combo không được để trống")
            .MaximumLength(200).WithMessage("Tên combo tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.DiscountAmount)
            .GreaterThan(0).WithMessage("Giảm giá cố định phải lớn hơn 0")
            .When(x => x.DiscountAmount.HasValue);

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0).WithMessage("Phần trăm giảm giá phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("Phần trăm giảm giá tối đa 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.DiscountAmount.HasValue && x.DiscountPercent.HasValue))
            .WithMessage("Chỉ được chỉ định một trong DiscountAmount hoặc DiscountPercent.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Combo phải có ít nhất một mục");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i)
                    .Must(i => (i.ProductId.HasValue ? 1 : 0) + (i.ServiceId.HasValue ? 1 : 0) == 1)
                    .WithMessage("Mỗi mục combo phải chỉ định đúng một trong ProductId hoặc ServiceId.");
            });
    }
}

public class UpdateGarageBundleRequestValidator : AbstractValidator<UpdateGarageBundleRequest>
{
    public UpdateGarageBundleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên combo không được để trống")
            .MaximumLength(200).WithMessage("Tên combo tối đa 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.DiscountAmount)
            .GreaterThan(0).WithMessage("Giảm giá cố định phải lớn hơn 0")
            .When(x => x.DiscountAmount.HasValue);

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0).WithMessage("Phần trăm giảm giá phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("Phần trăm giảm giá tối đa 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.DiscountAmount.HasValue && x.DiscountPercent.HasValue))
            .WithMessage("Chỉ được chỉ định một trong DiscountAmount hoặc DiscountPercent.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Combo phải có ít nhất một mục");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i)
                    .Must(i => (i.ProductId.HasValue ? 1 : 0) + (i.ServiceId.HasValue ? 1 : 0) == 1)
                    .WithMessage("Mỗi mục combo phải chỉ định đúng một trong ProductId hoặc ServiceId.");
            });
    }
}

public class UpdateGarageBundleStatusRequestValidator : AbstractValidator<UpdateGarageBundleStatusRequest>
{
    public UpdateGarageBundleStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái combo không hợp lệ");
    }
}

