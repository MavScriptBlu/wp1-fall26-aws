using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

// Records every PutObjectAsync call so tests can inspect bucket, key,
// content type, and metadata without touching real AWS.
public sealed class FakeS3ObjectClient : IS3ObjectClient
{
    public sealed record Call(string Bucket, string Key, string ContentType, IReadOnlyDictionary<string, string> Metadata);

    public List<Call> Calls { get; } = new();

    public Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        Calls.Add(new Call(bucket, key, contentType, metadata));
        return Task.CompletedTask;
    }
}
