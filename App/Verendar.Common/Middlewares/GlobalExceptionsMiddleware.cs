using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Verendar.Common.Shared;

namespace Verendar.Common.Middlewares
{
    public class GlobalExceptionsMiddleware(RequestDelegate next, ILogger<GlobalExceptionsMiddleware> logger, IHostEnvironment environment)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionsMiddleware> _logger = logger;
        private readonly IHostEnvironment _environment = environment;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            response.StatusCode = (int)statusCode;

            var message = _environment.IsDevelopment()
                ? exception.Message
                : "Đã xảy ra lỗi không mong muốn.";

            var apiResponse = ApiResponse<object>.FailureResponse(message);
            var result = JsonSerializer.Serialize(apiResponse);

            await response.WriteAsync(result);
        }
    }
}
