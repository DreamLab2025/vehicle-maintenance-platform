using System.Buffers;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Verendar.Common.Jwt;
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
                options.AddPolicy("DevelopmentCors", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });

                var allowedOrigins = builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>()
                    ?? builder.Configuration["Cors:AllowedOrigins"]?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(o => o.Trim())
                        .ToArray()
                    ?? [];

                options.AddPolicy("ProductionCors", policy =>
                {
                    if (allowedOrigins.Length > 0)
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    else
                        policy.SetIsOriginAllowed(_ => false);
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("Fixed", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10,
                        }));

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
                    var queueNamePrefix = ResolveQueueNamePrefix(builder, entryAssembly);

                    var rabbitMqHostSettings = BuildRabbitMqHostSettings(rabbitMqConnectionString);

                    cfg.Host(
                        rabbitMqHostSettings.Host,
                        rabbitMqHostSettings.Port,
                        rabbitMqHostSettings.VirtualHost,
                        hostConfigurator =>
                        {
                            if (!string.IsNullOrWhiteSpace(rabbitMqHostSettings.Username))
                            {
                                hostConfigurator.Username(rabbitMqHostSettings.Username);
                            }

                            if (!string.IsNullOrWhiteSpace(rabbitMqHostSettings.Password))
                            {
                                hostConfigurator.Password(rabbitMqHostSettings.Password);
                            }
                        });

                    if (string.IsNullOrWhiteSpace(queueNamePrefix))
                    {
                        cfg.ConfigureEndpoints(context);
                    }
                    else
                    {
                        cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(queueNamePrefix, false));
                    }

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

            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
            });

            builder.AddJwtAuthentication();
            builder.Services.AddSingleton<IServiceTokenProvider, ServiceTokenProvider>();
            builder.AddDefaultSwagger();

            return builder;
        }

        private static readonly SearchValues<char> HostSectionDelimiters = SearchValues.Create(['/', '?']);

        private static string ResolveQueueNamePrefix(IHostApplicationBuilder builder, Assembly? entryAssembly)
        {
            var serviceName = ResolveServiceName(builder.Environment.ApplicationName, entryAssembly);
            var configuredPrefix = builder.Configuration["MassTransit:QueueNamePrefix"];

            if (string.IsNullOrWhiteSpace(configuredPrefix))
            {
                return serviceName;
            }

            var interpolatedPrefix = configuredPrefix.Replace("{serviceName}", serviceName, StringComparison.OrdinalIgnoreCase);
            return NormalizeQueueSegment(interpolatedPrefix);
        }

        private static string ResolveServiceName(string? applicationName, Assembly? entryAssembly)
        {
            var rawName = string.IsNullOrWhiteSpace(applicationName)
                ? entryAssembly?.GetName().Name ?? string.Empty
                : applicationName;

            const string verendarPrefix = "Verendar.";
            if (rawName.StartsWith(verendarPrefix, StringComparison.OrdinalIgnoreCase))
            {
                rawName = rawName[verendarPrefix.Length..];
            }

            return NormalizeQueueSegment(rawName);
        }

        private static string NormalizeQueueSegment(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (char.IsUpper(c) &&
                        builder.Length > 0 &&
                        builder[^1] != '-')
                    {
                        builder.Append('-');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                    continue;
                }

                if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }
            }

            return builder.ToString().Trim('-');
        }

        private static Uri BuildRabbitMqUri(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new UriFormatException("RabbitMQ connection string is empty.");

            var normalizedConnectionString = NormalizeConnectionString(connectionString);

            if (Uri.TryCreate(normalizedConnectionString, UriKind.Absolute, out var validUri) &&
                !string.IsNullOrWhiteSpace(validUri.Host) &&
                IsSupportedRabbitMqScheme(validUri.Scheme))
            {
                return validUri;
            }

            var recoveredUri = TryRecoverUriFromUnescapedCredentials(normalizedConnectionString);
            if (recoveredUri != null)
            {
                return recoveredUri;
            }

            throw new UriFormatException(
                "RabbitMQ connection string is invalid. Expected format: amqp://user:password@host:port[/vhost]");
        }

        private static RabbitMqHostSettings BuildRabbitMqHostSettings(string connectionString)
        {
            var uri = BuildRabbitMqUri(connectionString);

            string? username = null;
            string? password = null;

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                var passwordSeparatorIndex = uri.UserInfo.IndexOf(':');
                if (passwordSeparatorIndex < 0)
                {
                    username = Uri.UnescapeDataString(uri.UserInfo);
                }
                else
                {
                    username = Uri.UnescapeDataString(uri.UserInfo[..passwordSeparatorIndex]);
                    password = Uri.UnescapeDataString(uri.UserInfo[(passwordSeparatorIndex + 1)..]);
                }
            }

            var port = uri.Port <= 0 ? GetDefaultRabbitMqPort(uri.Scheme) : uri.Port;
            if (port is < 1 or > ushort.MaxValue)
            {
                throw new UriFormatException($"RabbitMQ connection string contains invalid port '{uri.Port}'.");
            }

            return new RabbitMqHostSettings(
                Host: uri.Host,
                Port: (ushort)port,
                VirtualHost: GetVirtualHost(uri.AbsolutePath),
                Username: username,
                Password: password);
        }

        private static string NormalizeConnectionString(string connectionString)
        {
            var normalized = connectionString.Trim();

            if (normalized.Length < 2)
                return normalized;

            var isWrappedInDoubleQuotes = normalized.StartsWith('"') && normalized.EndsWith('"');
            var isWrappedInSingleQuotes = normalized.StartsWith('\'') && normalized.EndsWith('\'');

            if (!isWrappedInDoubleQuotes && !isWrappedInSingleQuotes)
                return normalized;

            return normalized[1..^1].Trim();
        }

        private static Uri? TryRecoverUriFromUnescapedCredentials(string connectionString)
        {
            var schemeSeparatorIndex = connectionString.IndexOf("://", StringComparison.Ordinal);
            if (schemeSeparatorIndex <= 0)
                return null;

            var scheme = connectionString[..schemeSeparatorIndex];
            if (!IsSupportedRabbitMqScheme(scheme))
            {
                return null;
            }

            var remainder = connectionString[(schemeSeparatorIndex + 3)..];

            // Find the last '@' first — passwords may contain '@' or '#', so we must not split on
            // URI special characters before we've located the credential boundary.
            var credentialsSeparatorIndex = remainder.LastIndexOf('@');
            if (credentialsSeparatorIndex <= 0 || credentialsSeparatorIndex == remainder.Length - 1)
                return null;

            var userInfo = remainder[..credentialsSeparatorIndex];
            var afterAt = remainder[(credentialsSeparatorIndex + 1)..];

            // Only '/' and '?' delimit the host section; '#' is excluded because it may appear in
            // an unescaped password that sits before the '@' we already found above.
            var hostEndIndex = afterAt.AsSpan().IndexOfAny(HostSectionDelimiters);
            var hostPort = hostEndIndex >= 0 ? afterAt[..hostEndIndex] : afterAt;
            var pathAndQuery = hostEndIndex >= 0 ? afterAt[hostEndIndex..] : string.Empty;
            var passwordSeparatorIndex = userInfo.IndexOf(':');
            var username = passwordSeparatorIndex >= 0 ? userInfo[..passwordSeparatorIndex] : userInfo;
            var password = passwordSeparatorIndex >= 0 ? userInfo[(passwordSeparatorIndex + 1)..] : string.Empty;

            var escapedUsername = Uri.EscapeDataString(username);
            var escapedPassword = string.IsNullOrEmpty(password)
                ? string.Empty
                : Uri.EscapeDataString(password);

            var rebuiltConnectionString = string.IsNullOrEmpty(password)
                ? $"{scheme}://{escapedUsername}@{hostPort}{pathAndQuery}"
                : $"{scheme}://{escapedUsername}:{escapedPassword}@{hostPort}{pathAndQuery}";

            if (Uri.TryCreate(rebuiltConnectionString, UriKind.Absolute, out var recoveredUri) &&
                !string.IsNullOrWhiteSpace(recoveredUri.Host))
            {
                return recoveredUri;
            }

            return null;
        }

        private static bool IsSupportedRabbitMqScheme(string scheme)
        {
            return scheme.Equals("amqp", StringComparison.OrdinalIgnoreCase) ||
                   scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase) ||
                   scheme.Equals("rabbitmq", StringComparison.OrdinalIgnoreCase) ||
                   scheme.Equals("rabbitmqs", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetDefaultRabbitMqPort(string scheme)
        {
            return scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase) ||
                   scheme.Equals("rabbitmqs", StringComparison.OrdinalIgnoreCase)
                ? 5671
                : 5672;
        }

        private static string GetVirtualHost(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath) || absolutePath == "/")
            {
                return "/";
            }

            var virtualHost = Uri.UnescapeDataString(absolutePath.TrimStart('/'));
            return string.IsNullOrWhiteSpace(virtualHost) ? "/" : virtualHost;
        }

        private sealed record RabbitMqHostSettings(
            string Host,
            ushort Port,
            string VirtualHost,
            string? Username,
            string? Password);

        public static WebApplication UseCommonService(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDefaultSwagger();
            }

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<HttpRequestLoggingMiddleware>();

            var corsPolicyName = app.Environment.IsDevelopment() ? "DevelopmentCors" : "ProductionCors";
            app.UseCors(corsPolicyName);
            app.UseMiddleware<GlobalExceptionsMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            return app;
        }
    }
}
