using Aspire.Hosting.Yarp;
using Scalar.Aspire;

namespace VMP.AppHost
{
    public static class ExternalServiceRegistrationExtensions
    {
        public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
        {
            var postgres = builder.AddPostgres("postgres")
                .WithContainerName("PostgresDb")
                .WithImageTag("17")
                .WithPgWeb(pgWeb =>
                {
                    pgWeb.WithContainerName("PgWeb")
                        .WithHostPort(5050);
                })
                .WithDataVolume();

            var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                .WithContainerName("Rabbitmq")
                .WithImageTag("3-management")
                .WithManagementPlugin(15672)
                .WithDataVolume();

            var identityDb = postgres.AddDatabase("identity-db", "Identities");

            var identityService = builder.AddProject<Projects.VMP_Identity>("vmp-identity")
                .WithReference(identityDb)
                .WithReference(rabbitMq)
                .WaitFor(postgres)
                .WaitFor(rabbitMq);

            var apiGateway = builder.AddYarp("api-gateway")
                            .WithContainerName("ApiGateway")
                            .WithHostPort(8080)
                            .WithConfiguration(yarp =>
                            {
                                yarp.AddRoute("/api/v1/identities/{**catch-all}", identityService);
                            })
                            .WaitFor(identityService);

            var scalarDocs = builder.AddScalarApiReference()
                .WithContainerName("ScalarDocs")
                .WithApiReference(identityService);

            return builder;
        }

        private static YarpRoute AddRoute(this IYarpConfigurationBuilder yarp, string path, IResourceBuilder<ProjectResource> resource)
        {
            var serviceCluster = yarp.AddCluster(resource).WithHttpClientConfig(
                new Yarp.ReverseProxy.Configuration.HttpClientConfig() { DangerousAcceptAnyServerCertificate = GetGatewayDangerousAcceptAnyServerCertificate() }
                );
            return yarp.AddRoute(path, serviceCluster);
        }

        private static bool GetGatewayDangerousAcceptAnyServerCertificate()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}