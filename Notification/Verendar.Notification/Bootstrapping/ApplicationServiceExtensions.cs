using Microsoft.AspNetCore.Authentication.JwtBearer;
using Polly;
using Polly.Extensions.Http;
using Verendar.Common.Bootstrapping;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Apis;
using Verendar.Notification.Application.Hubs;
using Verendar.Notification.Infrastructure.Configuration;
using Verendar.Notification.Infrastructure.Data;
using Verendar.Notification.Infrastructure.ExternalServices.Resend;
using Verendar.Notification.Infrastructure.Repositories.Implements;
using Verendar.Notification.Infrastructure.Services;
using Verendar.Notification.Application.Options;
using Verendar.Notification.Application.Services.Implements;
using Verendar.ServiceDefaults;

namespace Verendar.Notification.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.AddCommonService();

        builder.AddPostgresDatabase<NotificationDbContext>(Const.NotificationDatabase);

        builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection(ResendOptions.SectionName));
        builder.Services.Configure<NotificationAppOptions>(
            builder.Configuration.GetSection(NotificationAppOptions.SectionName));

        builder.Services.AddMemoryCache();

        var enableRealEmail = builder.Configuration.GetValue<bool>("Email:EnableRealSend");

        if (enableRealEmail)
        {
            builder.Services.AddHttpClient<IResendEmailService, ResendEmailService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
        }
        else
        {
            builder.Services.AddScoped<IResendEmailService, DevLogEmailService>();
        }

        builder.Services.AddSingleton<IEmailTemplateService, SimpleEmailTemplateService>();

        builder.Services.AddScoped<EmailChannel>();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.Services.AddScoped<IInAppNotificationService, InAppNotificationService>();

        builder.Services.AddSignalR();

        builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var onMessageReceived = options.Events?.OnMessageReceived;
            options.Events ??= new JwtBearerEvents();
            options.Events.OnMessageReceived = context =>
            {
                if (context.Request.Path.StartsWithSegments("/hubs"))
                {
                    var token = context.Request.Query["access_token"].FirstOrDefault()
                        ?? context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(token))
                        context.Token = token;
                }
                return onMessageReceived?.Invoke(context) ?? Task.CompletedTask;
            };
        });

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();

        app.UseCommonService();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapNotificationApi();

        return app;
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
