using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;

namespace Verendar.Common.Middlewares
{
    public class HttpRequestLoggingMiddleware(RequestDelegate next, ILogger<HttpRequestLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<HttpRequestLoggingMiddleware> _logger = logger;

        private static readonly string[] SkippedPaths = ["/health", "/alive"];

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (SkippedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // HTTP request/response only; skip WebSocket upgrade (long-lived, not a typical API round-trip).
            if (context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            var statusCode = context.Response.StatusCode;
            var clientIp = GetClientIp(context);
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var userAgent = context.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua
                ? (ua.Length > 200 ? ua[..200] : ua)
                : "-";
            var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;

            if (statusCode >= 500)
                _logger.LogError(
                    "HTTP {Method} {Path}{Query} responded {StatusCode} in {DurationMs}ms | ip={ClientIp} user={UserId} ua={UserAgent}",
                    context.Request.Method, path, query, statusCode, sw.ElapsedMilliseconds, clientIp, userId, userAgent);
            else if (statusCode >= 400)
                _logger.LogWarning(
                    "HTTP {Method} {Path}{Query} responded {StatusCode} in {DurationMs}ms | ip={ClientIp} user={UserId} ua={UserAgent}",
                    context.Request.Method, path, query, statusCode, sw.ElapsedMilliseconds, clientIp, userId, userAgent);
            else
                _logger.LogInformation(
                    "HTTP {Method} {Path}{Query} responded {StatusCode} in {DurationMs}ms | ip={ClientIp} user={UserId} ua={UserAgent}",
                    context.Request.Method, path, query, statusCode, sw.ElapsedMilliseconds, clientIp, userId, userAgent);
        }

        private static string GetClientIp(HttpContext context) =>
            context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
