using Verendar.Common.Shared;

namespace Verendar.Identity.Tests.Support;

internal static class IdentityServiceResponseAssert
{
    public static void AssertCreated<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(201);
        response.Message.Should().Be(expectedMessage);
        response.Metadata.Should().BeNull();
    }

    public static void AssertSuccess<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().NotBeNull();
    }

    public static void AssertFailure<T>(ApiResponse<T> response, int expectedStatusCode, string expectedMessage)
    {
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(expectedStatusCode);
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().BeNull();
        response.Metadata.Should().BeNull();
    }

    public static PagingMetadata AssertPagedSuccess<T>(
        ApiResponse<List<T>> response,
        string expectedMessage,
        int expectedItemCount,
        int expectedTotal)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().HaveCount(expectedItemCount);
        response.Metadata.Should().NotBeNull();
        var meta = response.Metadata.Should().BeOfType<PagingMetadata>().Subject;
        meta.TotalItems.Should().Be(expectedTotal);
        return meta;
    }
}
