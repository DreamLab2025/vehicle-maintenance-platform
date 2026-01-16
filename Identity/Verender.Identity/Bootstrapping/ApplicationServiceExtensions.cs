using Verender.Common.Bootstrapping;
using Verender.Common.Shared;
using Verender.Identity.Apis;
using Verender.Identity.Data;
using Verender.Identity.Repositories.Implements;
using Verender.Identity.Repositories.Interfaces;
using Verender.Identity.Services.Implements;
using Verender.Identity.Services.Interfaces;
using Verender.ServiceDefaults;

namespace Verender.Identity.Bootstrapping
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

            return app;
        }
    }
}
