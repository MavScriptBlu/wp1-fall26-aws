namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Options for configuring the S3 storage.
/// </summary>
public sealed class S3StorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = "documents/uploads";
    public string Region { get; set; } = "us-east-1";
    public long MaxFileSizeBytes { get; set; } = 1048576;
}
