using System.Net;
using FluentAssertions;
using Verendar.AppHost.Tests.Fixtures;

namespace Verendar.AppHost.Tests.Support;

internal static class TestDataInitializer
{
    public static async Task InitializeAsync(AppHostFixture fixture)
    {
        _ = await GatewayAuthHelper.LoginAndGetTokensAsync(
            fixture.IdentityClient,
            "user@gmail.com",
            "12345@Abc");

        var adminTokens = await GatewayAuthHelper.LoginAndGetTokensAsync(
            fixture.IdentityClient,
            "admin@gmail.com",
            "12345@Abc");

        adminTokens.AccessToken.Should().NotBeNullOrWhiteSpace();
        await EnsureGarageReferenceDataAsync(fixture);
    }

    private static async Task EnsureGarageReferenceDataAsync(AppHostFixture fixture)
    {
        using var categories = await fixture.GarageClient.GetAsync("/api/v1/service-categories");
        categories.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
