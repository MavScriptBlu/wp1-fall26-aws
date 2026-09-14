using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Wp1Fall26Aws.Storage;
using Xunit;

namespace Wp1Fall26Aws.Tests;

public class S3DocumentStoreTests
{
    private static S3DocumentStore MakeStore(FakeS3ObjectClient fake, S3StorageOptions? options = null)
    {
        options ??= new S3StorageOptions { BucketName = "test-bucket" };
        return new S3DocumentStore(fake, Options.Create(options));
    }

    private static DocumentUpload Upload(string fileName = "notes.txt", string contentType = "text/plain", long size = 11)
    {
        return new DocumentUpload(fileName, contentType, size, new MemoryStream(Encoding.UTF8.GetBytes("hello world")));
    }

    [Fact]
    public async Task StoreAsync_KeyMatchesExpectedFormat()
    {
        var fake = new FakeS3ObjectClient();
        var store = MakeStore(fake);

        var result = await store.StoreAsync(Upload());

        Assert.True(result.Success);
        Assert.Matches(new Regex(@"^documents/uploads/\d{4}/\d{2}/\d{2}/[0-9a-f]{32}\.txt$"), result.ObjectKey);
    }

    [Fact]
    public async Task StoreAsync_TwoUploadsOfSameFileNameProduceDifferentKeys()
    {
        var fake = new FakeS3ObjectClient();
        var store = MakeStore(fake);

        var first = await store.StoreAsync(Upload());
        var second = await store.StoreAsync(Upload());

        Assert.NotEqual(first.ObjectKey, second.ObjectKey);
    }

    [Fact]
    public async Task StoreAsync_AttachesAllFourMetadataEntries()
    {
        var fake = new FakeS3ObjectClient();
        var store = MakeStore(fake);

        await store.StoreAsync(Upload(fileName: "notes.txt", contentType: "text/plain", size: 11));

        var call = fake.Calls.Single();
        Assert.Equal("notes.txt", call.Metadata["original-filename"]);
        Assert.Equal("text/plain", call.Metadata["content-type"]);
        Assert.Equal("11", call.Metadata["content-length"]);
        Assert.True(DateTime.TryParse(call.Metadata["uploaded-utc"], out _));
    }

    [Fact]
    public async Task StoreAsync_RejectedUploadCallsTheFakeZeroTimes()
    {
        var fake = new FakeS3ObjectClient();
        var store = MakeStore(fake);

        var result = await store.StoreAsync(Upload(fileName: "virus.exe", contentType: "application/octet-stream"));

        Assert.False(result.Success);
        Assert.Empty(fake.Calls);
    }

    [Fact]
    public async Task StoreAsync_UsesTheConfiguredBucketName()
    {
        var fake = new FakeS3ObjectClient();
        var store = MakeStore(fake, new S3StorageOptions { BucketName = "my-custom-bucket" });

        await store.StoreAsync(Upload());

        Assert.Equal("my-custom-bucket", fake.Calls.Single().Bucket);
    }
}
