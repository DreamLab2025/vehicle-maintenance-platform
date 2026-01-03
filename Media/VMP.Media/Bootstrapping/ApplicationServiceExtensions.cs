using Amazon.S3;
using Microsoft.Extensions.Options;
using VMP.Common.Bootstrapping;
using VMP.Common.Shared;
using VMP.Media.Apis;
using VMP.Media.Application.Configuration;
using VMP.Media.Application.IStorage;
using VMP.Media.Application.Services.Implements;
using VMP.Media.Application.Services.Interfaces;
using VMP.Media.Domain.Repositories.Interfaces;
using VMP.Media.Infrastructure.Configuration;
using VMP.Media.Infrastructure.Data;
using VMP.Media.Infrastructure.Repositories.Implements;
using VMP.Media.Infrastructure.Storage;
using VMP.ServiceDefaults;

namespace VMP.Media.Bootstrapping
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
