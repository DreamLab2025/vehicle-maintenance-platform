using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.Json.Serialization;
using Verendar.Common.Middlewares;
using Verendar.Common.Shared;
using Verendar.ServiceDefaults;

namespace Verendar.Common.Bootstrapping
{
    public static class CommonApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddCommonService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllClients", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(policyName: "Fixed", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                });
            });

            builder.Services.Configure<RateLimiterOptions>(options =>
            {
                options.RejectionStatusCode = 429;
            });

            builder.Services.AddMassTransit(config =>
            {
                var scannedAssemblies = new HashSet<Assembly>();
                var entryAssembly = Assembly.GetEntryAssembly();

                if (entryAssembly != null)
                {
                    config.AddConsumers(entryAssembly);
                    scannedAssemblies.Add(entryAssembly);
                }

                var assembliesToScan = GetRelevantAssemblies(entryAssembly)
                    .Where(a => !scannedAssemblies.Contains(a))
                    .ToList();

                // Scan each assembly for consumers
                foreach (var assembly in assembliesToScan)
                {
                    if (HasConsumers(assembly))
                    {
                        config.AddConsumers(assembly);
                        scannedAssemblies.Add(assembly);
                    }
                }

                static IEnumerable<Assembly> GetRelevantAssemblies(Assembly? entryAssembly)
                {
                    var assemblies = new HashSet<Assembly>();

                    // Get already-loaded assemblies
                    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic && IsApplicationAssembly(a))
                        .ToList();

                    foreach (var assembly in loadedAssemblies)
                    {
                        assemblies.Add(assembly);
                    }

                    // Try to load referenced assemblies that might contain consumers
                    if (entryAssembly != null)
                    {
                        foreach (var assemblyName in entryAssembly.GetReferencedAssemblies())
                        {
                            if (!IsApplicationAssemblyName(assemblyName)) continue;

                            try
                            {
                                var assembly = Assembly.Load(assemblyName);
                                assemblies.Add(assembly);
                            }
                            catch
                            {
                            }
                        }
                    }

                    return assemblies;
                }

                static bool IsApplicationAssembly(Assembly assembly)
                {
                    var name = assembly.FullName;
                    if (string.IsNullOrEmpty(name)) return false;

                    return !name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("MassTransit", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("Newtonsoft", StringComparison.OrdinalIgnoreCase);
                }

                static bool IsApplicationAssemblyName(AssemblyName assemblyName)
                {
                    var name = assemblyName.FullName;
                    if (string.IsNullOrEmpty(name)) return false;

                    return !name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("MassTransit", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("Newtonsoft", StringComparison.OrdinalIgnoreCase);
                }

                static bool HasConsumers(Assembly assembly)
                {
                    try
                    {
                        return assembly.GetTypes()
                            .Any(t => !t.IsAbstract &&
                                     !t.IsInterface &&
                                     t.GetInterfaces()
                                         .Any(i => i.IsGenericType &&
                                                  i.GetGenericTypeDefinition() == typeof(IConsumer<>)));
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }

                config.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConnectionString = builder.Configuration.GetConnectionString(Const.RabbitMQ)
                        ?? throw new ArgumentNullException(nameof(builder.Configuration), "RabbitMq connection string is empty");

                    cfg.Host(new Uri(rabbitMqConnectionString));

                    cfg.ConfigureEndpoints(context);

                    cfg.UseMessageRetry(retry =>
                    {
                        retry.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5)
                        );
                    });
                });
            });

            // Configure JSON serialization for Minimal APIs to use string enums
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.AddJwtAuthentication();
            builder.AddDefaultSwagger();

            return builder;
        }

        public static WebApplication UseCommonService(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDefaultSwagger();
            }
            
            app.UseCors("AllowAllClients");
            app.UseMiddleware<GlobalExceptionsMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            return app;
        }
    }
}
