using Verendar.Common.Shared;

namespace Verendar.Garage.Tests.Support;

internal static class GarageServiceResponseAssert
{
    public static void AssertCreatedEnvelope<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(201);
        response.Message.Should().Be(expectedMessage);
        response.Metadata.Should().BeNull();
    }

    public static void AssertFailureEnvelope<T>(ApiResponse<T> response, int expectedStatusCode, string expectedMessage)
    {
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(expectedStatusCode);
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().BeNull();
        response.Metadata.Should().BeNull();
    }
}
