using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;

namespace VMP.ServiceDefaults
{
    public static class SwaggerExtensions
    {
        public static IHostApplicationBuilder AddDefaultSwagger(this IHostApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token JWT"
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();

                options.SchemaFilter<EnumSchemaFilter>();

                options.OperationFilter<EnumParameterOperationFilter>();

                options.UseInlineDefinitionsForEnums();

                var xmlFilename = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            });

            return builder;
        }

        public static WebApplication UseDefaultSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "VMP API Documentation";
                c.DefaultModelsExpandDepth(1);
                c.DisplayRequestDuration();
                c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
                {
                    ["activated"] = true,
                    ["theme"] = "agate"
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
            if (metadata.Any(x => x is IAuthorizeData))
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
                    }
                };
            }
        }
    }

    public class EnumParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;

            foreach (var parameter in operation.Parameters)
            {
                var apiDesc = context.ApiDescription.ParameterDescriptions
                    .FirstOrDefault(p => p.Name == parameter.Name);

                var type = apiDesc?.Type;

                if (type != null && Nullable.GetUnderlyingType(type) is Type uType)
                {
                    type = uType;
                }

                if (type != null && type.IsEnum)
                {
                    if (!string.IsNullOrEmpty(parameter.Description) &&
                       (parameter.Description.Contains("Options:") || parameter.Description.Contains("<ul>")))
                    {
                        continue;
                    }

                    var enumDescriptions = new List<string>();
                    foreach (var name in Enum.GetNames(type))
                    {
                        var memberInfo = type.GetMember(name).FirstOrDefault();
                        var descriptionAttr = memberInfo?.GetCustomAttribute<DescriptionAttribute>();

                        if (descriptionAttr != null)
                        {
                            enumDescriptions.Add($"<b>{name}</b>: {descriptionAttr.Description}");
                        }
                    }

                    if (enumDescriptions.Any())
                    {
                        var descHtml = "<br/>Options:<ul><li>" + string.Join("</li><li>", enumDescriptions) + "</li></ul>";
                        parameter.Description += descHtml;
                    }
                }
            }
        }
    }

    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            if (type != null && type.IsEnum)
            {
                schema.Enum.Clear();
                schema.Type = "string";
                schema.Format = null;
                var enumDescriptions = new List<string>();

                foreach (var name in Enum.GetNames(type))
                {
                    schema.Enum.Add(new OpenApiString(name));
                    var memberInfo = type.GetMember(name).FirstOrDefault();
                    var descriptionAttr = memberInfo?.GetCustomAttribute<DescriptionAttribute>();
                    if (descriptionAttr != null)
                        enumDescriptions.Add($"<b>{name}</b>: {descriptionAttr.Description}");
                }

                if (enumDescriptions.Any())
                {
                    schema.Description += "<p>Values:</p><ul><li>" + string.Join("</li><li>", enumDescriptions) + "</li></ul>";
                }
            }
        }
    }
}