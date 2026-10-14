using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

/// <summary>
/// A spy for the S3 boundary, used by the integration tests. No network, pretends
/// every upload worked, and remembers the last call so a test can check what the
/// app tried to send to S3. (FakeS3ObjectClient keeps a full list for the unit tests;
/// this one only needs the last call.)
/// </summary>
public sealed class RecordingS3ObjectClientSpy : IS3ObjectClient
{
    public sealed record PutCall(string Bucket, string Key, string ContentType);

    /// <summary>The last upload the app tried to send to S3, or null if nothing was sent.</summary>
    public PutCall? LastRequest { get; private set; }

    public Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        LastRequest = new PutCall(bucket, key, contentType); // remember what was "uploaded"
        return Task.CompletedTask;                          // pretend S3 said yes
    }
}
