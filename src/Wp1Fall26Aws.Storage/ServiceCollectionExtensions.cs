using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// DI wiring for the storage layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="S3StorageOptions"/> from config and registers the S3 client
    /// and document store so the app can inject <see cref="IDocumentStore"/>.
    /// </summary>
    public static IServiceCollection AddDocumentStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3StorageOptions>(configuration.GetSection("S3Storage"));
        services.AddScoped<IS3ObjectClient, AwsS3ObjectClient>();
        services.AddScoped<IDocumentStore, S3DocumentStore>();
        return services;
    }
}
