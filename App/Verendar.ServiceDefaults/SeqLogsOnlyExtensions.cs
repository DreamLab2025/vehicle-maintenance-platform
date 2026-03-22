using Aspire.Seq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace Verendar.ServiceDefaults;

internal static class SeqLogsOnlyExtensions
{
    private const string DefaultConfigSectionName = "Aspire:Seq";

    public static void AddSeqLogsOnlyEndpoint(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<SeqSettings>? configureSettings = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(connectionName);

        var settings = new SeqSettings();
        settings.Logs.Protocol = OtlpExportProtocol.HttpProtobuf;
        settings.Logs.ExportProcessorType = ExportProcessorType.Batch;

        builder.Configuration.GetSection(DefaultConfigSectionName).Bind(settings);
        builder.Configuration.GetSection(connectionName).Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is { } connectionString)
            settings.ServerUrl = connectionString;

        configureSettings?.Invoke(settings);

        if (string.IsNullOrEmpty(settings.ServerUrl))
        {
            return;
        }

        settings.Logs.Endpoint = new Uri($"{settings.ServerUrl}/ingest/otlp/v1/logs");

        if (!string.IsNullOrEmpty(settings.ApiKey))
        {
            settings.Logs.Headers = string.IsNullOrEmpty(settings.Logs.Headers)
                ? $"X-Seq-ApiKey={settings.ApiKey}"
                : $"{settings.Logs.Headers},X-Seq-ApiKey={settings.ApiKey}";
        }

        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddProcessor(
            _ => settings.Logs.ExportProcessorType switch
            {
                ExportProcessorType.Batch => new BatchLogRecordExportProcessor(new OtlpLogExporter(settings.Logs)),
                _ => new SimpleLogRecordExportProcessor(new OtlpLogExporter(settings.Logs))
            }));

        if (!settings.DisableHealthChecks)
        {
            var seqBase = settings.ServerUrl;
            builder.Services.AddHealthChecks().AddAsyncCheck("Seq", async (ct) =>
            {
                using var client = new HttpClient(new SocketsHttpHandler { ActivityHeadersPropagator = null })
                {
                    BaseAddress = new Uri(seqBase)
                };
                using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), ct)
                    .ConfigureAwait(false);
                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy();
            });
        }
    }
}
