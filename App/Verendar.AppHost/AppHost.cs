using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Verendar.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.Configure<ResourceNotificationServiceOptions>(o =>
{
    o.DefaultWaitBehavior = WaitBehavior.WaitOnResourceUnavailable;
});

builder.AddApplicationServices();

builder.Build().Run();

