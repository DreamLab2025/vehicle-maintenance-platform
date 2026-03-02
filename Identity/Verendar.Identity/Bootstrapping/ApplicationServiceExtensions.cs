using FluentValidation;
using Verendar.Common.Bootstrapping;
using Verendar.Common.Shared;
using Verendar.Identity.Apis;
using Verendar.Identity.Application.Validators;
using Verendar.Identity.Infrastructure.Data;
using Verendar.Identity.Infrastructure.Repositories.Implements;
using Verendar.Identity.Infrastructure.Services;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verendar.Identity.Application.Services.Interfaces;
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

            builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

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

            app.MapAuthApi();
            app.MapUserApi();
            app.MapInternalUserApi();

            return app;
        }
    }
}
