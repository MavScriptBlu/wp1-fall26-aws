namespace Wp1Fall26Aws.Storage;

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
