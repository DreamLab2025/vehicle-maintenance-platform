namespace Verendar.Ai.Tests.Support;

using Verendar.Common.Shared;

internal static class AiServiceResponseAssert
{
    public static void AssertSuccessEnvelope<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be(expectedMessage);
        response.Metadata.Should().BeNull();
    }

    public static void AssertSuccessEnvelope<T>(ApiResponse<T> response)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
    }

    public static void AssertFailureEnvelope<T>(ApiResponse<T> response, int expectedStatusCode, string expectedMessage)
    {
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(expectedStatusCode);
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().BeNull();
        response.Metadata.Should().BeNull();
    }

    public static void AssertFailureEnvelope<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().BeNull();
    }

    public static void AssertSuccessPagedEnvelope<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be(expectedMessage);
        response.Metadata.Should().NotBeNull();
    }
}
