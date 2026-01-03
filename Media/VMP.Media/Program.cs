using VMP.Media.Bootstrapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.UseApplicationServices();

app.Run();
