namespace Verendar.Garage.Tests.Support;

internal sealed class TestHttpMessageHandler : HttpMessageHandler
{
    public required Func<HttpRequestMessage, HttpResponseMessage> SendImpl { get; init; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(SendImpl(request));
}
