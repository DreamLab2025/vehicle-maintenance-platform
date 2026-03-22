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
                .WithContainerName("PostgresDb")
                .WithImage("postgres", "17.4-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume();

            if (string.Equals(Environment.GetEnvironmentVariable(PgAdminEnvVar), "1", StringComparison.OrdinalIgnoreCase))
            {
                postgres = postgres.WithPgAdmin(pgAdmin =>
                {
                    pgAdmin.WithContainerName("PgAdmin")
                        .WithHostPort(5050)
                        .WithLifetime(ContainerLifetime.Persistent);
                });
            }

            var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                .WithContainerName("Rabbitmq")
                .WithImageTag("3-management-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin(15672)
                .WithDataVolume();

            var redis = builder.AddRedis("redis-cache")
                .WithImage("redis", "7.4-alpine")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent);

            var seq = builder.AddSeq("seq")
                .WithImage("datalust/seq", "2025.2")
                .WithImagePullPolicy(ImagePullPolicy.Missing)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume()
                .ExcludeFromManifest();

            var identityDb = postgres.AddDatabase("identity-db", "Identities");
            var vehicleDb = postgres.AddDatabase("vehicle-db", "Vehicles");
            var mediaDb = postgres.AddDatabase("media-db", "Media");
            var notificationDb = postgres.AddDatabase("notification-db", "Notifications");
            var aiDb = postgres.AddDatabase("ai-db", "Ais");

            var identityService = builder.AddProject<Projects.Verendar_Identity>("Verendar-identity")
                .WithReference(identityDb)
                .WithReference(rabbitMq)
                .WithReference(redis);
            identityService = identityService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq)
                .WaitFor(redis);

            var vehicleService = builder.AddProject<Projects.Verendar_Vehicle>("Verendar-vehicle")
                .WithReference(vehicleDb)
                .WithReference(rabbitMq);
            vehicleService = vehicleService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var mediaService = builder.AddProject<Projects.Verendar_Media>("Verendar-media")
                .WithReference(mediaDb)
                .WithReference(rabbitMq);
            mediaService = mediaService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var notificationService = builder.AddProject<Projects.Verendar_Notification>("Verendar-notification")
                .WithReference(notificationDb)
                .WithReference(rabbitMq);
            notificationService = notificationService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var aiService = builder.AddProject<Projects.Verendar_Ai>("Verendar-ai")
                .WithReference(aiDb)
                .WithReference(rabbitMq);
            aiService = aiService
                .WithReference(seq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);


            var apiGateway = builder.AddYarp("api-gateway")
                .WithContainerName("ApiGateway")
                .WithHostPort(8080)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithConfiguration(yarp =>
                {
                    var identityCluster = yarp.AddProjectCluster(identityService);
                    yarp.AddRoute("/api/v1/auth/{**catch-all}", identityCluster);
                    yarp.AddRoute("/api/v1/users/{**catch-all}", identityCluster);
                    yarp.AddRoute("/api/internal/{**catch-all}", identityCluster);

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
                })
                .WaitFor(identityService)
                .WaitFor(vehicleService)
                .WaitFor(mediaService)
                .WaitFor(notificationService)
                .WaitFor(aiService);

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