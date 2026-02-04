using Verendar.Common.Bootstrapping;
using Verendar.Common.Shared;
using Verendar.Identity.Apis;
using Verendar.Identity.Data;
using Verendar.Identity.Repositories.Implements;
using Verendar.Identity.Repositories.Interfaces;
using Verendar.Identity.Services.Implements;
using Verendar.Identity.Services.Interfaces;
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

            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();

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
