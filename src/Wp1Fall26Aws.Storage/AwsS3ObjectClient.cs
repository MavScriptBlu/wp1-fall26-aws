using Amazon.S3;
using Amazon.S3.Model;

namespace Wp1Fall26Aws.Storage;

// The thin real implementation wrapping IAmazonS3. This is the only class
// in the project that touches the AWS SDK directly.
public sealed class AwsS3ObjectClient : IS3ObjectClient
{
    private readonly IAmazonS3 _s3;

    public AwsS3ObjectClient(IAmazonS3 s3)
    {
        _s3 = s3;
    }

    public async Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        foreach (var kvp in metadata)
        {
            request.Metadata.Add(kvp.Key, kvp.Value);
        }

        await _s3.PutObjectAsync(request, ct);
    }
}
