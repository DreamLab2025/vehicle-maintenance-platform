using Aspire.Hosting.Yarp;
using Yarp.ReverseProxy.Configuration;

namespace VMP.AppHost.Extensions
{
    public static class ExternalServiceRegistrationExtensions
    {
        public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
        {
            var postgres = builder.AddPostgres("postgres")
                .WithContainerName("PostgresDb")
                .WithImageTag("17")
                .WithDataVolume()
                .WithPgAdmin(pgAdmin =>
                {
                    pgAdmin.WithContainerName("PgAdmin")
                           .WithHostPort(5050);
                });

            var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                .WithContainerName("Rabbitmq")
                .WithImageTag("3-management")
                .WithManagementPlugin(15672)
                .WithDataVolume();

            var identityDb = postgres.AddDatabase("identity-db", "Identities");
            var vehicleDb = postgres.AddDatabase("vehicle-db", "Vehicles");

            var identityService = builder.AddProject<Projects.VMP_Identity>("vmp-identity")
                .WithReference(identityDb)
                .WithReference(rabbitMq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var vehicleService = builder.AddProject<Projects.VMP_Vehicle>("vmp-vehicle")
                .WithReference(vehicleDb)
                .WithReference(rabbitMq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var apiGateway = builder.AddYarp("api-gateway")
                            .WithContainerName("ApiGateway")
                            .WithHostPort(8080)
                            .WithConfiguration(yarp =>
                            {
                                var identityCluster = yarp.AddProjectCluster(identityService);
                                yarp.AddRoute("/api/v1/auth/{**catch-all}", identityCluster);
                                yarp.AddRoute("/api/v1/identities/{**catch-all}", identityCluster);

                                var vehicleCluster = yarp.AddProjectCluster(vehicleService);
                                yarp.AddRoute("/api/v1/brands/{**catch-all}", vehicleCluster);
                                yarp.AddRoute("/api/v1/vehicles/{**catch-all}", vehicleCluster);
                            })
                            .WaitFor(identityService);

            //var scalarDocs = builder.AddScalarApiReference()
            //    .WithContainerName("ScalarDocs")
            //    .WithApiReference(identityService);

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