using FluentValidation;
using Verendar.Common.Bootstrapping;
using Verendar.Identity.Apis;
using Verendar.Identity.Application.Validators;
using Verendar.Identity.Infrastructure.Data;
using Verendar.Identity.Infrastructure.Repositories.Implements;
using Verendar.Identity.Application.Services.Implements;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verendar.ServiceDefaults;

namespace Verendar.Identity.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.AddCommonService();

            builder.AddPostgresDatabase<UserDbContext>(Const.IdentityDatabase);

            builder.AddServiceRedis(nameof(Identity), connectionName: Const.Redis);

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IIdentityStatsService, IdentityStatsService>();

            builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

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

            app.MapAuthApi();
            app.MapUserApi();
            app.MapInternalUserApi();
            app.MapFeedbackApi();
            app.MapIdentityStatsApi();

            return app;
        }
    }
}
