using VMP.Common.Bootstrapping;
using VMP.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddCommonService();

builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCommonService();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
