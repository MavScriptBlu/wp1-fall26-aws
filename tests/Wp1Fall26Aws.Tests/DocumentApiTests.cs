using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

// IClassFixture: xUnit builds ONE WebApplicationFactory and shares it across the tests in this class.
public sealed class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task PostDocuments_ValidTextFile_Returns201AndSendsToS3()
    {
        // Arrange: swap the real S3 client for the fake so nothing touches AWS.
        var fake = new FakeS3ObjectClient();
        var client = factory.WithWebHostBuilder(builder =>
        {
            // ConfigureTestServices runs AFTER Program.cs, so these registrations win.
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IS3ObjectClient>(fake);
                // The store refuses to upload without a bucket name, so give it a fake one.
                services.PostConfigure<S3StorageOptions>(options => options.BucketName = "integration-test-bucket");
            });
        }).CreateClient();

        var fileContent = new ByteArrayContent("synthetic lab file"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", "synthetic-document.txt"); // field name "file" is what the endpoint reads

        // Act: a real HTTP POST, handled entirely in memory.
        var response = await client.PostAsync("/documents", form);

        // Assert: the HTTP contract AND the side effect at the S3 boundary.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("documents/uploads/", body);

        var call = Assert.Single(fake.Calls);
        Assert.Equal("integration-test-bucket", call.Bucket);
        Assert.Equal("text/plain", call.ContentType);
    }
}
