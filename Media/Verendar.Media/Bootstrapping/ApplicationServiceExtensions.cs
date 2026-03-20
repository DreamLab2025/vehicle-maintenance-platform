using Amazon.S3;
using FluentValidation;
using Microsoft.Extensions.Options;
using Verendar.Common.Bootstrapping;
using Verendar.Common.Shared;
using Verendar.Media.Apis;
using Verendar.Media.Application.Configuration;
using Verendar.Media.Application.Dtos;
using Verendar.Media.Application.IStorage;
using Verendar.Media.Application.Services.Implements;
using Verendar.Media.Application.Services.Interfaces;
using Verendar.Media.Application.Validators;
using Verendar.Media.Domain.Repositories.Interfaces;
using Verendar.Media.Infrastructure.Configuration;
using Verendar.Media.Infrastructure.Data;
using Verendar.Media.Infrastructure.Repositories.Implements;
using Verendar.Media.Infrastructure.Storage;
using Verendar.ServiceDefaults;

namespace Verendar.Media.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.AddCommonService();

            builder.AddPostgresDatabase<MediaDbContext>(Const.MediaDatabase);

            builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(settings.Region)
                };

                if (!string.IsNullOrEmpty(settings.AccessKey) && !string.IsNullOrEmpty(settings.SecretKey))
                {
                    return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
                }

                return new AmazonS3Client(config);
            });
            builder.Services.AddScoped<IStorageService, AwsS3StorageService>();

            // Register Configuration
            builder.Services.Configure<FileUploadConfiguration>(
                builder.Configuration.GetSection(FileUploadConfiguration.SectionName));

            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            builder.Services.AddScoped<IMediaUploadService, MediaUploadService>();
            builder.Services.AddScoped<IValidator<InitUploadRequest>, InitUploadRequestValidator>();

            return builder;
        }

        public static WebApplication UseApplicationServices(this WebApplication app)
        {
            app.MapDefaultEndpoints();

            app.UseCommonService();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.MapMediaFileApi();

            return app;
        }
    }
}
