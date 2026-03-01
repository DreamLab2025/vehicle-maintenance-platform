using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Verendar.Ai.Application.Handlers
{
    public sealed class ForwardAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ForwardAuthorizationHandler> logger) : DelegatingHandler
    {
        private const string AuthorizationHeaderName = "Authorization";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "ForwardAuthorizationHandler: Processing request to {RequestUri}. Current Authorization header: {HasAuth}",
                request.RequestUri,
                request.Headers.Authorization != null ? "Present" : "Missing");

            if (request.Headers.Authorization == null)
            {
                await TryForwardAuthorizationAsync(request);
            }
            else
            {
                logger.LogInformation(
                    "ForwardAuthorizationHandler: Authorization header already present on request to {RequestUri}, skipping forward.",
                    request.RequestUri);
            }

            var response = await base.SendAsync(request, cancellationToken);

            logger.LogInformation(
                "ForwardAuthorizationHandler: Response from {RequestUri}: {StatusCode}. Authorization was: {HadAuth}",
                request.RequestUri,
                response.StatusCode,
                request.Headers.Authorization != null ? "Present" : "Missing");

            return response;
        }

        private async Task TryForwardAuthorizationAsync(HttpRequestMessage request)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                logger.LogWarning(
                    "ForwardAuthorizationHandler: HttpContext is null. Outgoing request to {RequestUri} will not include Authorization.",
                    request.RequestUri);
                return;
            }

            logger.LogInformation(
                "ForwardAuthorizationHandler: HttpContext found. Checking for Authorization header. Request path: {Path}, User authenticated: {IsAuthenticated}",
                httpContext.Request.Path,
                httpContext.User?.Identity?.IsAuthenticated ?? false);

            string? authValue = null;

            if (httpContext.Request.Headers.TryGetValue(AuthorizationHeaderName, out var authHeader))
            {
                authValue = authHeader.ToString();
                logger.LogInformation(
                    "ForwardAuthorizationHandler: Found Authorization header via TryGetValue. Length: {Length}",
                    authValue?.Length ?? 0);
            }

            if (string.IsNullOrWhiteSpace(authValue))
            {
                var authenticateResult = await httpContext.AuthenticateAsync();
                if (authenticateResult?.Succeeded == true && authenticateResult.Properties?.Items.TryGetValue(".Token.access_token", out var token) == true)
                {
                    authValue = $"Bearer {token}";
                    logger.LogInformation("ForwardAuthorizationHandler: Found token from authentication result");
                }
            }

            if (string.IsNullOrWhiteSpace(authValue) && httpContext.User?.Identity?.IsAuthenticated == true)
            {
                logger.LogWarning(
                    "ForwardAuthorizationHandler: User is authenticated but no Authorization header found. User: {UserId}. This might indicate the header was consumed by JWT middleware.",
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Unknown");
            }

            if (string.IsNullOrWhiteSpace(authValue))
            {
                logger.LogWarning(
                    "ForwardAuthorizationHandler: No Authorization header or token found. Available headers: {Headers}. Outgoing request to {RequestUri} will not include Authorization.",
                    string.Join(", ", httpContext.Request.Headers.Keys),
                    request.RequestUri);
                return;
            }

            if (!AuthenticationHeaderValue.TryParse(authValue, out var parsed) || string.IsNullOrEmpty(parsed.Parameter))
            {
                logger.LogWarning(
                    "ForwardAuthorizationHandler: Authorization header could not be parsed. Value length: {Length}, Value preview: {Preview}. Outgoing request to {RequestUri} will not include Authorization.",
                    authValue.Length,
                    authValue.Length > 20 ? authValue.Substring(0, 20) + "..." : authValue,
                    request.RequestUri);
                return;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue(parsed.Scheme, parsed.Parameter);
            logger.LogInformation(
                "ForwardAuthorizationHandler: Successfully forwarded Bearer token (scheme: {Scheme}, parameter length: {Length}) to {RequestUri}",
                parsed.Scheme,
                parsed.Parameter?.Length ?? 0,
                request.RequestUri);
        }
    }
}
