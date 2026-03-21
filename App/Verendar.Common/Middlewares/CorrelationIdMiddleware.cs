using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;

namespace Verendar.Common.Middlewares
{
    public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[Const.CorrelationIdHeaderName].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");

            context.Items[Const.CorrelationIdKey] = correlationId;
            context.Response.Headers[Const.CorrelationIdHeaderName] = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object> { [Const.CorrelationIdKey] = correlationId }))
            {
                await _next(context);
            }
        }
    }
}
