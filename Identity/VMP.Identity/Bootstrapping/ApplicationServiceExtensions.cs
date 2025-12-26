using VMP.Common.Bootstrapping;
using VMP.Common.Shared;
using VMP.Identity.Apis;
using VMP.Identity.Data;
using VMP.Identity.Repositories.Implements;
using VMP.Identity.Repositories.Interfaces;
using VMP.Identity.Services.Implement;
using VMP.Identity.Services.Implements;
using VMP.Identity.Services.Interfaces;

namespace VMP.Identity.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.AddCommonService();

            builder.AddPostgresDatabase<UserDbContext>(Const.IdentityDatabase);

            builder.Services.AddScoped<IUserRepository, UserRepository>();

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
