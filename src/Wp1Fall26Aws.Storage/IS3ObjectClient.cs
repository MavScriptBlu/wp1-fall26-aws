namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Interface for a client that can interact with S3 objects.
/// </summary>
public interface IS3ObjectClient
{
    Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default);
}
