using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Verendar.AppHost.Tests.Support;

internal static class GatewayAuthHelper
{
    public sealed record LoginTokens(string AccessToken, string RefreshToken);

    public static async Task<LoginTokens> LoginAndGetTokensAsync(
        HttpClient apiGatewayClient,
        string email = "user@gmail.com",
        string password = "12345@Abc")
    {
        var loginResponse = await apiGatewayClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await loginResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);

        document.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        data.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
        data.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
        accessToken.GetString().Should().NotBeNullOrWhiteSpace();
        refreshToken.GetString().Should().NotBeNullOrWhiteSpace();

        return new LoginTokens(accessToken.GetString()!, refreshToken.GetString()!);
    }

    public static async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpClient apiGatewayClient,
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await apiGatewayClient.SendAsync(request);
    }

    public static async Task AssertUnauthorizedAsync(HttpClient apiGatewayClient, Func<Task<HttpResponseMessage>> send)
    {
        _ = apiGatewayClient;

        using var response = await ExecuteWithTransientRetryAsync(send);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static void AssertNotAuthFailure(HttpStatusCode statusCode)
    {
        statusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        statusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    private static async Task<HttpResponseMessage> ExecuteWithTransientRetryAsync(Func<Task<HttpResponseMessage>> send)
    {
        const int maxAttempts = 3;
        var delayMs = 150;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await send();
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts && IsTransientGatewayAbort(ex))
            {
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }

        return await send();
    }

    private static bool IsTransientGatewayAbort(HttpRequestException ex)
    {
        if (ex.InnerException is HttpIOException)
        {
            return true;
        }

        if (ex.InnerException is IOException ioEx &&
            ioEx.Message.Contains("connection was aborted", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ex.Message.Contains("response ended prematurely", StringComparison.OrdinalIgnoreCase);
    }
}
