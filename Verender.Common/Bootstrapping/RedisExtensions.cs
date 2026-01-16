using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Verender.Common.Caching;
using Verender.Common.Shared;

namespace Verender.Common.Bootstrapping
{
    public static class RedisExtensions
    {
        private const string DefaultConnectionName = Const.Redis;

        public static IHostApplicationBuilder AddServiceRedis(this IHostApplicationBuilder builder, string serviceName, string connectionName = DefaultConnectionName)
        {
            builder.AddRedisClient(connectionName);

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