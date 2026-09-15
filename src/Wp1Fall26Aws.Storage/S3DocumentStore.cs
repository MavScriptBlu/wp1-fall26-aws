

using Microsoft.Extensions.Options;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// S3-backed <see cref="IDocumentStore"/>. Validates an upload, then writes it to the
/// configured bucket under a date-partitioned key.
/// </summary>
public sealed class S3DocumentStore : IDocumentStore
{
    private readonly IS3ObjectClient _client;
    private readonly S3StorageOptions _options;

    public S3DocumentStore(IS3ObjectClient client, IOptions<S3StorageOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    /// <summary>
    /// Validates the upload and, if it passes, stores it in S3. Returns the validation
    /// errors instead of throwing when the upload is rejected.
    /// </summary>
    public async Task<StoreResult> StoreAsync(DocumentUpload upload, CancellationToken ct = default)
    {
        var errors = DocumentValidator.Validate(upload, _options);
        if (errors.Count > 0)
        {
            return new StoreResult(false, null, errors, null);
        }

        var key = BuildKey(upload.FileName);
        var metadata = BuildMetadata(upload);

        await _client.PutObjectAsync(_options.BucketName, key, upload.Content, upload.ContentType, metadata, ct);

        return new StoreResult(true, key, Array.Empty<string>(), metadata);
    }

    /// <summary>
    /// Builds the S3 object key: "{prefix}/yyyy/MM/dd/{new guid}{original extension}".
    /// The date partition keeps a bucket from growing one flat, unbrowsable folder.
    /// </summary>
    private string BuildKey(string fileName)
    {
        var now = DateTime.UtcNow;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var id = Guid.NewGuid().ToString("N");
        return $"{_options.KeyPrefix}/{now:yyyy}/{now:MM}/{now:dd}/{id}{ext}";
    }

    /// <summary>
    /// Captures the upload's original filename, content type, size, and upload time
    /// as S3 object metadata, since the generated key doesn't preserve any of that.
    /// </summary>
    private static IReadOnlyDictionary<string, string> BuildMetadata(DocumentUpload upload)
    {
        return new Dictionary<string, string>
        {
            ["original-filename"] = upload.FileName,
            ["content-type"] = upload.ContentType,
            ["uploaded-utc"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["content-length"] = upload.ContentLength.ToString()
        };
    }
}
