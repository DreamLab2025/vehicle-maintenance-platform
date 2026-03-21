using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Verendar.Common.Jwt;

namespace Verendar.Common.Bootstrapping
{
    public static class JwtAuthenticationExtensions
    {
        public static IHostApplicationBuilder AddJwtAuthentication(this IHostApplicationBuilder builder)
        {
            var jwtOptions = builder.Configuration
                .GetSection(JwtBearerConfigurationOptions.SectionName)
                .Get<JwtBearerConfigurationOptions>()
                ?? throw new InvalidOperationException($"JWT configuration section '{JwtBearerConfigurationOptions.SectionName}' not found in appsettings.json");

            if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey cannot be null or empty");
            }

            builder.Services.Configure<JwtBearerConfigurationOptions>(
                builder.Configuration.GetSection(JwtBearerConfigurationOptions.SectionName));

            builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

                options.SaveToken = true;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtOptions.ValidateIssuer,
                    ValidateAudience = jwtOptions.ValidateAudience,
                    ValidateLifetime = jwtOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidProgramException>>();
                        logger.LogError(context.Exception, "JWT Authentication Failed for request: {Path}",
                            context.Request.Path);

                        if (context.Exception is SecurityTokenInvalidAudienceException)
                        {
                            logger.LogError("Lỗi Audience: Token Audience không khớp với Config.");
                        }
                        else if (context.Exception is SecurityTokenInvalidIssuerException)
                        {
                            logger.LogError("Lỗi Issuer: Token Issuer không khớp với Config.");
                        }
                        else if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                            logger.LogError("Lỗi Hết hạn: Token đã hết hạn.");
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var errorMessage = "You are not authorized to access this resource";

                        return context.Response.WriteAsJsonAsync(new
                        {
                            error = "Unauthorized",
                            message = errorMessage
                        });
                    }
                };
            });

            builder.Services.AddAuthorization();

            return builder;
        }
    }
}
