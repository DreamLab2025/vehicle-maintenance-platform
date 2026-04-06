using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Yarp;
using Scalar.Aspire;
using Yarp.ReverseProxy.Configuration;

namespace Verendar.AppHost.Extensions
{
    public static class ExternalServiceRegistrationExtensions
    {
        public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
        {
            var isDevelopment = builder.Environment.IsDevelopment();
            var isIsolatedTestRun = IsIsolatedTestRun();
            var runSuffix = isIsolatedTestRun
                ? $"-test-{Guid.NewGuid():N}".Substring(0, 14)
                : string.Empty;

            var postgres = builder.AddPostgres("postgres")
                .WithContainerName($"verendar-aspire-postgres{runSuffix}")
                .WithImage("postgres", "17.4-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(isIsolatedTestRun ? ContainerLifetime.Session : ContainerLifetime.Persistent);

            if (!isIsolatedTestRun)
            {
                postgres = postgres.WithDataVolume();
            }

            if (isDevelopment && !isIsolatedTestRun)
            {
                postgres = postgres.WithPgWeb(pgWeb =>
                {
                    pgWeb.WithContainerName("verendar-aspire-pgweb")
                        .WithHostPort(5050)
                        .WithLifetime(ContainerLifetime.Persistent);
                });
            }

            var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                .WithContainerName($"verendar-aspire-rabbitmq{runSuffix}")
                .WithImageTag("3-management-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(isIsolatedTestRun ? ContainerLifetime.Session : ContainerLifetime.Persistent)
                .WithManagementPlugin(isIsolatedTestRun ? null : 15672);

            if (!isIsolatedTestRun)
            {
                rabbitMq = rabbitMq.WithDataVolume();
            }

            var redis = builder.AddRedis("redis")
                .WithImage("redis", "7.4-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(isIsolatedTestRun ? ContainerLifetime.Session : ContainerLifetime.Persistent)
                .WithContainerName($"verendar-aspire-redis{runSuffix}");

            var seq = builder.AddSeq("seq")
                .WithImage("datalust/seq", "2025.2")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(isIsolatedTestRun ? ContainerLifetime.Session : ContainerLifetime.Persistent)
                .WithContainerName($"verendar-aspire-seq{runSuffix}")
                .ExcludeFromManifest();

            if (!isIsolatedTestRun)
            {
                seq = seq.WithDataVolume();
            }

            var identityDb = postgres.AddDatabase("identity-db", "Identities");
            var vehicleDb = postgres.AddDatabase("vehicle-db", "Vehicles");
            var mediaDb = postgres.AddDatabase("media-db", "Media");
            var notificationDb = postgres.AddDatabase("notification-db", "Notifications");
            var aiDb = postgres.AddDatabase("ai-db", "Ais");
            var locationDb = postgres.AddDatabase("location-db", "Locations");
            var garageDb = postgres.AddDatabase("garage-db", "Garages");

            var identityService = builder.AddProject<Projects.Verendar_Identity>("identity-service")
                .WithReference(identityDb)
                .WithReference(rabbitMq)
                .WithReference(redis);
            identityService = identityService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq)
                .WaitFor(redis);

            var vehicleService = builder.AddProject<Projects.Verendar_Vehicle>("vehicle-service")
                .WithReference(vehicleDb)
                .WithReference(rabbitMq)
                .WithReference(identityService);
            vehicleService = vehicleService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var mediaService = builder.AddProject<Projects.Verendar_Media>("media-service")
                .WithReference(mediaDb)
                .WithReference(rabbitMq);
            mediaService = mediaService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var notificationService = builder.AddProject<Projects.Verendar_Notification>("notification-service")
                .WithReference(notificationDb)
                .WithReference(rabbitMq);
            notificationService = notificationService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var aiService = builder.AddProject<Projects.Verendar_Ai>("ai-service")
                .WithReference(aiDb)
                .WithReference(rabbitMq)
                .WithReference(redis);
            aiService = aiService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq)
                .WaitFor(redis);

            var locationService = builder.AddProject<Projects.Verendar_Location>("location-service")
                .WithReference(locationDb)
                .WithReference(redis)
                .WithReference(rabbitMq);
            locationService = locationService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(redis)
                .WaitFor(rabbitMq);

            var garageService = builder.AddProject<Projects.Verendar_Garage>("garage-service")
                .WithReference(garageDb)
                .WithReference(rabbitMq)
                .WithReference(locationService);
            garageService = garageService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq)
                .WaitFor(locationService);


            var apiGateway = builder.AddYarp("api-gateway")
                .WithContainerName($"verendar-aspire-gateway{runSuffix}")
                .WithLifetime(isIsolatedTestRun ? ContainerLifetime.Session : ContainerLifetime.Persistent)
                .WithConfiguration(yarp =>
                {
                    var identityCluster = yarp.AddProjectCluster(identityService);
                    yarp.AddRoute("/api/v1/auth/{**catch-all}", identityCluster);
                    yarp.AddRoute("/api/v1/users/{**catch-all}", identityCluster);
                    yarp.AddRoute("/api/v1/feedback/{**catch-all}", identityCluster);
                    yarp.AddRoute("/api/v1/identity/{**catch-all}", identityCluster);

                    var aiCluster = yarp.AddProjectCluster(aiService);
                    yarp.AddRoute("/api/v1/ai/{**catch-all}", aiCluster);

                    var vehicleCluster = yarp.AddProjectCluster(vehicleService);
                    yarp.AddRoute("/api/v1/user-vehicles/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/maintenance-records/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/variants/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/models/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/vehicle-models/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/odometer-history/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/part-categories/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/types/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/brands/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/vehicle/{**catch-all}", vehicleCluster);

                    var mediaCluster = yarp.AddProjectCluster(mediaService);
                    yarp.AddRoute("/api/v1/media-files/{**catch-all}", mediaCluster);

                    var notificationCluster = yarp.AddProjectCluster(notificationService);
                    yarp.AddRoute("/api/v1/notifications/{**catch-all}", notificationCluster);
                    yarp.AddRoute("/hubs/{**catch-all}", notificationCluster);

                    var locationCluster = yarp.AddProjectCluster(locationService);
                    yarp.AddRoute("/api/v1/locations/{**catch-all}", locationCluster);

                    var garageCluster = yarp.AddProjectCluster(garageService);
                    yarp.AddRoute("/api/v1/garages/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/bookings/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/garage-products/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/garage-services/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/garage-bundles/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/service-categories/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/garage-catalog/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/members/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/branches/{**catch-all}", garageCluster);
                    yarp.AddRoute("/api/v1/garage/{**catch-all}", garageCluster);
                })
                .WaitFor(identityService)
                .WaitFor(vehicleService)
                .WaitFor(mediaService)
                .WaitFor(notificationService)
                .WaitFor(aiService)
                .WaitFor(locationService)
                .WaitFor(garageService);

            if (!isIsolatedTestRun)
            {
                apiGateway = apiGateway.WithHostPort(8080);
            }

            if (isDevelopment)
            {
                var scalarDocs = builder.AddScalarApiReference("api-docs", configureOptions: options => options
                    .PreferHttpsEndpoint()
                    .AllowSelfSignedCertificates()
                    .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                    .WithOperationTitleSource(OperationTitleSource.Path)
                    .AddPreferredSecuritySchemes("Bearer"))
                    .WithApiReference(identityService)
                    .WithApiReference(vehicleService)
                    .WithApiReference(mediaService)
                    .WithApiReference(notificationService)
                    .WithApiReference(aiService)
                    .WithApiReference(locationService)
                    .WithApiReference(garageService);

                if (!isIsolatedTestRun)
                {
                    scalarDocs = scalarDocs.WithEndpoint("http", endpoint => endpoint.Port = 8081);
                }
            }

            return builder;
        }

        private static bool IsIsolatedTestRun()
        {
            var isolatedFlag = Environment.GetEnvironmentVariable("VERENDAR_TEST_ISOLATED");
            return string.Equals(isolatedFlag, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(isolatedFlag, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static YarpCluster AddProjectCluster(this IYarpConfigurationBuilder yarp, IResourceBuilder<ProjectResource> resource)
        {
            return yarp.AddCluster(resource).WithHttpClientConfig(new HttpClientConfig
            {
                DangerousAcceptAnyServerCertificate = GetGatewayDangerousAcceptAnyServerCertificate()
            });
        }

        private static YarpRoute AddRoute(this IYarpConfigurationBuilder yarp, string path, IResourceBuilder<ProjectResource> resource)
        {
            var serviceCluster = yarp.AddCluster(resource).WithHttpClientConfig(new HttpClientConfig()
            {
                DangerousAcceptAnyServerCertificate = GetGatewayDangerousAcceptAnyServerCertificate()
            });
            return yarp.AddRoute(path, serviceCluster);
        }

        private static bool GetGatewayDangerousAcceptAnyServerCertificate()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}