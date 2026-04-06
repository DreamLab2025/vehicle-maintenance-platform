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

namespace Verendar.ServiceDefaults
{
    public static class SwaggerExtensions
    {
        public static IHostApplicationBuilder AddDefaultSwagger(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenApi();

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
                options.OperationFilter<RequestBodyExampleOperationFilter>();

                options.UseInlineDefinitionsForEnums();

                options.DocumentFilter<ApiErrorEnvelopeDocumentFilter>();

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
                c.DocumentTitle = "Verendar API Documentation";
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

    public sealed class ApiErrorEnvelopeDocumentFilter : IDocumentFilter
    {
        public const string SchemaId = "ApiErrorEnvelope";

        private static readonly HashSet<string> KnownErrorCodes = new(StringComparer.Ordinal)
        {
            "default", "400", "401", "403", "404", "405", "409", "415", "422", "429", "500", "502", "503"
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Components ??= new OpenApiComponents();
            swaggerDoc.Components.Schemas ??= new Dictionary<string, OpenApiSchema>(StringComparer.Ordinal);
            if (!swaggerDoc.Components.Schemas.ContainsKey(SchemaId))
            {
                swaggerDoc.Components.Schemas[SchemaId] = new OpenApiSchema
                {
                    Type = "object",
                    Description = "Unsuccessful API envelope. Runtime uses the same JSON shape as success, but <c>data</c> is always null for these status codes.",
                    Properties = new Dictionary<string, OpenApiSchema>(StringComparer.Ordinal)
                    {
                        ["isSuccess"] = new OpenApiSchema { Type = "boolean", Example = new OpenApiBoolean(false) },
                        ["statusCode"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                        ["message"] = new OpenApiSchema { Type = "string" },
                        ["data"] = new OpenApiSchema
                        {
                            Nullable = true,
                            Description = "Always null when isSuccess is false."
                        },
                        ["metadata"] = new OpenApiSchema
                        {
                            Nullable = true,
                            Description = "Optional; only some errors include metadata."
                        }
                    }
                };
            }

            if (swaggerDoc.Paths == null) return;

            foreach (var pathItem in swaggerDoc.Paths.Values)
            {
                foreach (var operation in pathItem.Operations.Values)
                {
                    if (operation.Responses == null) continue;
                    foreach (var (code, response) in operation.Responses)
                    {
                        if (!IsErrorStatusCode(code)) continue;
                        if (response.Content == null) continue;
                        foreach (var media in response.Content.Values)
                        {
                            if (media.Schema == null) continue;
                            if (!LooksLikeApiResponseEnvelope(media.Schema, swaggerDoc)) continue;
                            media.Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = SchemaId
                                }
                            };
                        }
                    }
                }
            }
        }

        private static bool IsErrorStatusCode(string code)
        {
            if (KnownErrorCodes.Contains(code)) return true;
            return int.TryParse(code, out var n) && n >= 400;
        }

        private static bool LooksLikeApiResponseEnvelope(OpenApiSchema schema, OpenApiDocument doc)
        {
            var resolved = ResolveSchema(schema, doc);
            if (resolved?.Properties == null) return false;
            return HasProp(resolved.Properties, "isSuccess")
                && HasProp(resolved.Properties, "statusCode")
                && HasProp(resolved.Properties, "message")
                && HasProp(resolved.Properties, "data");
        }

        private static bool HasProp(IDictionary<string, OpenApiSchema> properties, string name) =>
            properties.Keys.Any(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));

        private static OpenApiSchema? ResolveSchema(OpenApiSchema schema, OpenApiDocument doc)
        {
            if (schema.Reference != null && doc.Components?.Schemas != null &&
                doc.Components.Schemas.TryGetValue(schema.Reference.Id, out var target))
            {
                return target;
            }

            return schema.Reference == null ? schema : null;
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

    public class RequestBodyExampleOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content == null) return;

            foreach (var mediaType in operation.RequestBody.Content.Values)
            {
                if (mediaType.Example != null || mediaType.Schema == null) continue;

                var example = OpenApiExampleFactory.CreateExample(mediaType.Schema, context.SchemaRepository, new HashSet<string>(StringComparer.Ordinal));
                if (example != null)
                {
                    mediaType.Example = example;
                }
            }
        }
    }

    internal static class OpenApiExampleFactory
    {
        public static IOpenApiAny? CreateExample(OpenApiSchema schema, SchemaRepository repository, HashSet<string> visitedRefs)
        {
            if (schema.Example != null) return schema.Example;

            if (schema.Reference?.Id is string refId)
            {
                if (!visitedRefs.Add(refId)) return null;
                if (repository.Schemas.TryGetValue(refId, out var refSchema))
                {
                    return CreateExample(refSchema, repository, visitedRefs);
                }
                return null;
            }

            if (schema.Enum != null && schema.Enum.Count > 0)
            {
                return schema.Enum[0];
            }

            if (schema.Type == "object" || schema.Properties.Count > 0)
            {
                var obj = new OpenApiObject();
                foreach (var (propertyName, propertySchema) in schema.Properties)
                {
                    var propertyExample = CreateExample(propertySchema, repository, visitedRefs);
                    if (propertyExample != null)
                    {
                        obj[propertyName] = propertyExample;
                    }
                }
                return obj;
            }

            if (schema.Type == "array")
            {
                var array = new OpenApiArray();
                if (schema.Items != null)
                {
                    var itemExample = CreateExample(schema.Items, repository, visitedRefs);
                    if (itemExample != null) array.Add(itemExample);
                }
                return array;
            }

            return schema.Type switch
            {
                "string" => new OpenApiString(schema.Format == "date-time" ? DateTime.UtcNow.ToString("O") : "string"),
                "integer" => new OpenApiInteger(0),
                "number" => new OpenApiDouble(0),
                "boolean" => new OpenApiBoolean(false),
                _ => null
            };
        }
    }
}