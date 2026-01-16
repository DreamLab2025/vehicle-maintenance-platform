using Verender.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.Build().Run();

