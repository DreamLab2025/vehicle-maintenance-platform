using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Verendar.Common.Caching;
using Verendar.Common.Shared;

namespace Verendar.Common.Bootstrapping
{
    public static class RedisExtensions
    {
        private const string DefaultConnectionName = Const.Redis;

        public static IHostApplicationBuilder AddServiceRedis(this IHostApplicationBuilder builder, string serviceName, string connectionName = DefaultConnectionName)
        {
            builder.AddRedisClient(connectionName);

            builder.Services.AddDataProtection()
                .SetApplicationName($"Verendar-{serviceName}");

            // Persist Data Protection keys to Redis so they survive container restarts.
            // Uses IConfigureOptions to defer IConnectionMultiplexer resolution until
            // after the DI container is built (avoids BuildServiceProvider anti-pattern).
            builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
                new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    var mux = sp.GetRequiredService<IConnectionMultiplexer>();
                    options.XmlRepository = new RedisXmlRepository(
                        () => mux.GetDatabase(),
                        $"DataProtection-Keys:{serviceName}");
                }));

            builder.Services.AddSingleton<ICacheService>(sp =>
            {
                var connection = sp.GetRequiredService<IConnectionMultiplexer>();
                var db = connection.GetDatabase();
                return new CacheService(db, serviceName);
            });

            return builder;
        }
    }
}
