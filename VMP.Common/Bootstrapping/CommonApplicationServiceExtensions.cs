using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.Json.Serialization;
using VMP.Common.Middlewares;
using VMP.Common.Shared;
using VMP.ServiceDefaults;

namespace VMP.Common.Bootstrapping
{
    public static class CommonApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddCommonService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllClients", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(policyName: "Fixed", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                });
            });

            builder.Services.Configure<RateLimiterOptions>(options =>
            {
                options.RejectionStatusCode = 429;
            });

            builder.Services.AddMassTransit(config =>
            {
                config.AddConsumers(Assembly.GetEntryAssembly());

                config.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConnectionString = builder.Configuration.GetConnectionString(Const.RabbitMQ)
                        ?? throw new ArgumentNullException(nameof(builder.Configuration), "RabbitMq connection string is empty");

                    cfg.Host(new Uri(rabbitMqConnectionString));

                    cfg.ConfigureEndpoints(context);

                    cfg.UseMessageRetry(retry =>
                    {
                        retry.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5)
                        );
                    });
                });
            });

            // Configure JSON serialization for Minimal APIs to use string enums
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.AddJwtAuthentication();
            builder.AddDefaultSwagger();

            return builder;
        }

        public static WebApplication UseCommonService(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDefaultSwagger();
            }
            app.UseCors("AllowAllClients");
            app.UseMiddleware<GlobalExceptionsMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            return app;
        }
    }
}
