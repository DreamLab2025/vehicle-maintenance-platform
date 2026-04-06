using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Validators;
using Verendar.Identity.Domain.Enums;

namespace Verendar.Identity.Tests.Validators;

public class CreateFeedbackRequestValidatorTests
{
    private readonly CreateFeedbackRequestValidator _validator = new();

    private static CreateFeedbackRequest Valid() => new()
    {
        Category = FeedbackCategory.Bug,
        Subject = "Tiêu đề hợp lệ",
        Content = "Nội dung phải có ít nhất 10 ký tự."
    };

    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        var result = await _validator.ValidateAsync(Valid());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Subject_WhenEmpty_Fails()
    {
        var request = Valid() with { Subject = "" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Subject));
    }

    [Fact]
    public async Task Subject_WhenTooShort_Fails()
    {
        var request = Valid() with { Subject = "ab" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Subject));
    }

    [Fact]
    public async Task Subject_WhenTooLong_Fails()
    {
        var request = Valid() with { Subject = new string('x', 201) };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Subject));
    }

    [Fact]
    public async Task Content_WhenEmpty_Fails()
    {
        var request = Valid() with { Content = "" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Content));
    }

    [Fact]
    public async Task Content_WhenTooShort_Fails()
    {
        var request = Valid() with { Content = "Ngắn quá" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Content));
    }

    [Fact]
    public async Task Content_WhenTooLong_Fails()
    {
        var request = Valid() with { Content = new string('x', 5001) };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Content));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Rating_WhenOutOfRange_Fails(int rating)
    {
        var request = Valid() with { Rating = rating };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Rating));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Rating_WhenInRange_Passes(int rating)
    {
        var request = Valid() with { Rating = rating };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Rating_WhenNull_Passes()
    {
        var request = Valid() with { Rating = null };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ContactEmail_WhenInvalidFormat_Fails()
    {
        var request = Valid() with { ContactEmail = "not-an-email" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ContactEmail));
    }

    [Fact]
    public async Task ContactEmail_WhenValidFormat_Passes()
    {
        var request = Valid() with { ContactEmail = "user@example.com" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ContactEmail_WhenNull_Passes()
    {
        var request = Valid() with { ContactEmail = null };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Category_WhenInvalidEnum_Fails()
    {
        var request = Valid() with { Category = (FeedbackCategory)99 };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Category));
    }
}
