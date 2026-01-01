using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VMP.ServiceDefaults
{
    public static class SwaggerExtensions
    {
        public static IHostApplicationBuilder AddDefaultSwagger(this IHostApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token JWT của bạn vào đây (chỉ nhập chuỗi token)"
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.SchemaFilter<EnumSchemaFilter>();
                
                // Force inline enum definitions instead of references
                options.UseInlineDefinitionsForEnums();
            });
            
            return builder;
        }

        public static WebApplication UseDefaultSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // Disable caching to ensure latest schema
                c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
                {
                    ["activated"] = false
                };
            });
            app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
            return app;
        }
    }

    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            var hasAuthorize = metadata.Any(x => x is IAuthorizeData);

            if (hasAuthorize)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    }
                };

                if (!operation.Responses.ContainsKey("401"))
                    operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            }
        }
    }

    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            
            // Handle nullable enums (e.g., VehicleTransmissionType?)
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }
            
            if (type != null && type.IsEnum)
            {
                schema.Enum.Clear();
                schema.Type = "string";
                schema.Format = null;
                schema.Nullable = context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Nullable<>);
                
                foreach (var enumValue in Enum.GetValues(type))
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue.ToString()));
                }
            }
        }
    }
}
