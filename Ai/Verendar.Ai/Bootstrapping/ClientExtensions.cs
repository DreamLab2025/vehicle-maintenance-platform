using Polly;
using Polly.Extensions.Http;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Infrastructure.Clients;
using Verendar.Common.Http;

namespace Verendar.Ai.Bootstrapping
{
    public static class ClientExtensions
    {
        public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(client =>
            {
                var baseUrl = builder.Configuration["VehicleService:BaseUrl"]
                    ?? builder.Configuration["Services:Vehicle:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<ForwardAuthorizationHandler>()
            .AddPolicyHandler(GetResiliencePolicy());

            return builder;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
        {
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }
    }
}