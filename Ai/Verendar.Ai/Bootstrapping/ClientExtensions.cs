using Polly;
using Polly.Extensions.Http;
using Verendar.Ai.Application.Clients;

namespace Verendar.Ai.Bootstrapping;

public static class ClientExtensions
{
    /// <summary>
    /// Register HTTP clients for external services
    /// </summary>
    public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
    {
        // Register Vehicle Service Client with Polly policies
        builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(client =>
        {
            var baseUrl = builder.Configuration["VehicleService:BaseUrl"]
                ?? builder.Configuration["Services:Vehicle:BaseUrl"]
                ?? "http://localhost:7002";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return builder;
    }


    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                });
    }


    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
