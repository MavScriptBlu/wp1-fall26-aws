using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wp1Fall26Aws.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3StorageOptions>(configuration.GetSection("S3Storage"));
        services.AddScoped<IS3ObjectClient, AwsS3ObjectClient>();
        services.AddScoped<IDocumentStore, S3DocumentStore>();
        return services;
    }
}
