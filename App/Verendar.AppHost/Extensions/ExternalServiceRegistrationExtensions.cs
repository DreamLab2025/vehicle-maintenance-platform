using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Yarp;
using Yarp.ReverseProxy.Configuration;

namespace Verendar.AppHost.Extensions
{
    public static class ExternalServiceRegistrationExtensions
    {
        private const string PgAdminEnvVar = "VERENDAR_PGADMIN";

        public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
        {
            var postgres = builder.AddPostgres("postgres")
                .WithContainerName("verendar-aspire-postgres")
                .WithImage("postgres", "17.4-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume();

            if (string.Equals(Environment.GetEnvironmentVariable(PgAdminEnvVar), "1", StringComparison.OrdinalIgnoreCase))
            {
                postgres = postgres.WithPgAdmin(pgAdmin =>
                {
                    pgAdmin.WithContainerName("verendar-aspire-pgadmin")
                        .WithHostPort(5050)
                        .WithLifetime(ContainerLifetime.Persistent);
                });
            }

            var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                .WithContainerName("verendar-aspire-rabbitmq")
                .WithImageTag("3-management-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin(15672)
                .WithDataVolume();

            var redis = builder.AddRedis("redis")
                .WithImage("redis", "7.4-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithContainerName("verendar-aspire-redis");

            var seq = builder.AddSeq("seq")
                .WithImage("datalust/seq", "2025.2")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume()
                .WithContainerName("verendar-aspire-seq")
                .ExcludeFromManifest();

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
                .WithReference(rabbitMq);
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
                .WithReference(rabbitMq);
            aiService = aiService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

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
                .WithReference(rabbitMq);
            garageService = garageService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);


            var apiGateway = builder.AddYarp("api-gateway")
                .WithContainerName("verendar-aspire-gateway")
                .WithHostPort(8080)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithConfiguration(yarp =>
                {
                    var identityCluster = yarp.AddProjectCluster(identityService);
                    yarp.AddRoute("/api/v1/auth/{**catch-all}", identityCluster);
                    yarp.AddRoute("/api/v1/users/{**catch-all}", identityCluster);

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
                    yarp.AddRoute("/api/v1/part-products/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/types/{**catch-all}", vehicleCluster);
                    yarp.AddRoute("/api/v1/brands/{**catch-all}", vehicleCluster);

                    var mediaCluster = yarp.AddProjectCluster(mediaService);
                    yarp.AddRoute("/api/v1/media-files/{**catch-all}", mediaCluster);

                    var notificationCluster = yarp.AddProjectCluster(notificationService);
                    yarp.AddRoute("/api/v1/notifications/{**catch-all}", notificationCluster);
                    yarp.AddRoute("/hubs/{**catch-all}", notificationCluster);

                    var locationCluster = yarp.AddProjectCluster(locationService);
                    yarp.AddRoute("/api/v1/locations/{**catch-all}", locationCluster);

                    var garageCluster = yarp.AddProjectCluster(garageService);
                    yarp.AddRoute("/api/v1/garages/{**catch-all}", garageCluster);
                })
                .WaitFor(identityService)
                .WaitFor(vehicleService)
                .WaitFor(mediaService)
                .WaitFor(notificationService)
                .WaitFor(aiService)
                .WaitFor(locationService)
                .WaitFor(garageService);

            return builder;
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