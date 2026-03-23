using Verendar.Garage.Application.Clients;
using Verendar.Garage.Infrastructure.Clients;

namespace Verendar.Garage.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.AddCommonService();

        builder.AddPostgresDatabase<GarageDbContext>(Const.GarageDatabase);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Payment:BaseUrl"];
            if (!string.IsNullOrEmpty(baseAddress))
                client.BaseAddress = new Uri(baseAddress);
        });

        builder.Services.AddHttpClient<IVietQRClient, VietQRHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.vietqr.io");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
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

        return app;
    }
}
