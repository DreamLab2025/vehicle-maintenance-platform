using Microsoft.AspNetCore.Authentication.JwtBearer;
using Polly;
using Polly.Extensions.Http;
using Verendar.Common.Bootstrapping;
using Verendar.Common.Shared;
using Verendar.Notification.Application.Dtos.ESms;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Apis;
using Verendar.Notification.Hubs;
using Verendar.Notification.Infrastructure.Configuration;
using Verendar.Notification.Infrastructure.Data;
using Verendar.Notification.Infrastructure.ExternalServices.ESms;
using Verendar.Notification.Infrastructure.ExternalServices.Resend;
using Verendar.Notification.Infrastructure.Repositories.Implements;
using Verendar.Notification.Infrastructure.Services;
using Verendar.Notification.Services;
using Verendar.ServiceDefaults;

namespace Verendar.Notification.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.AddCommonService();

            builder.AddPostgresDatabase<NotificationDbContext>(Const.NotificationDatabase);

            builder.Services.Configure<ESmsOptions>(builder.Configuration.GetSection("ESms"));
            builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection(ResendOptions.SectionName));

            // Add Memory Cache for template caching
            builder.Services.AddMemoryCache();

            builder.Services.AddHttpClient<IESmsService, ESmsService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Resend Email Service - Using HttpClient directly with Resend REST API
            builder.Services.AddHttpClient<IResendEmailService, ResendEmailService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            builder.Services.AddSingleton<IEmailTemplateService, RazorEmailTemplateService>();

            // Notification Channels
            builder.Services.AddScoped<INotificationChannel, SmsChannel>();
            builder.Services.AddScoped<INotificationChannel, ZaloChannel>();
            builder.Services.AddScoped<INotificationChannel, EmailChannel>();
            builder.Services.AddScoped<IChannelFactory, ChannelFactory>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddScoped<IInAppNotificationService, InAppNotificationService>();

            // SignalR: userId lấy từ JWT (mặc định dùng claim NameIdentifier / "sub" giống CurrentUserService trong Common)
            builder.Services.AddSignalR();

            // JWT cho SignalR: WebSocket không gửi header Authorization, đọc token từ query "access_token"
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

            app.UseCommonService();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

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
}
