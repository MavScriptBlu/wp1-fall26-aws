using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

/// <summary>
/// Integration tests: boots the real API in memory and sends it real HTTP requests.
/// The only thing swapped out is the S3 client, so no AWS login is needed.
/// </summary>
// IClassFixture: xUnit makes ONE factory and shares it across every test in this class.
public sealed class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// Builds a client for an app whose S3 client is replaced by the given spy.
    /// ConfigureTestServices runs AFTER Program.cs, so the last registration (ours) wins.
    /// </summary>
    private HttpClient CreateClientWithSpy(RecordingS3ObjectClientSpy spy)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IS3ObjectClient>(spy);
                services.PostConfigure<S3StorageOptions>(options => options.BucketName = "integration-test-bucket");
            });
        }).CreateClient();
    }

    /// <summary>Builds a multipart/form-data body with one file in the "file" field, like a browser upload.</summary>
    private static MultipartFormDataContent CreateUpload(string fileName, string contentType, byte[] bytes)
    {
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", fileName);
        return form;
    }

    [Fact]
    public async Task GetHealth_AppRunning_ReturnsOk()
    {
        // Arrange: /health never touches S3, so a plain client is fine.
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostDocuments_ValidTextFile_Returns201AndSendsToS3()
    {
        // Arrange
        var spy = new RecordingS3ObjectClientSpy();
        var client = CreateClientWithSpy(spy);
        using var upload = CreateUpload("synthetic-document.txt", "text/plain", "synthetic lab file"u8.ToArray());

        // Act
        var response = await client.PostAsync("/documents", upload);

        // Assert: the HTTP answer AND the side effect at the S3 boundary.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(spy.LastRequest);
        Assert.Equal("integration-test-bucket", spy.LastRequest!.Bucket);
        Assert.Equal("text/plain", spy.LastRequest.ContentType);
    }

    [Fact]
    public async Task PostDocuments_ShellScript_Returns400AndNeverCallsS3()
    {
        // Arrange
        var spy = new RecordingS3ObjectClientSpy();
        var client = CreateClientWithSpy(spy);
        using var upload = CreateUpload("script.sh", "text/x-shellscript", "echo hi"u8.ToArray());

        // Act
        var response = await client.PostAsync("/documents", upload);

        // Assert: rejected, and nothing reached S3.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(spy.LastRequest);
    }
}
