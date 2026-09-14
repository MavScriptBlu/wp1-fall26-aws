using Microsoft.Extensions.Options;

namespace Wp1Fall26Aws.Storage;

public sealed class S3DocumentStore : IDocumentStore
{
    private readonly IS3ObjectClient _client;
    private readonly S3StorageOptions _options;

    public S3DocumentStore(IS3ObjectClient client, IOptions<S3StorageOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

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

    private string BuildKey(string fileName)
    {
        var now = DateTime.UtcNow;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var id = Guid.NewGuid().ToString("N");
        return $"{_options.KeyPrefix}/{now:yyyy}/{now:MM}/{now:dd}/{id}{ext}";
    }

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
