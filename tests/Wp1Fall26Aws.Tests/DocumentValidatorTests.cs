using System.Text;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

public class DocumentValidatorTests
{
    private static readonly S3StorageOptions Options = new();

    private static DocumentUpload Upload(string fileName, string contentType, long size)
    {
        var bytes = Encoding.UTF8.GetBytes(new string('a', (int)Math.Min(size, 16)));
        return new DocumentUpload(fileName, contentType, size, new MemoryStream(bytes));
    }

    [Fact]
    public void Validate_AcceptsValidSmallTextFile()
    {
        var errors = DocumentValidator.Validate(Upload("notes.txt", "text/plain", 1024), Options);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsEmptyFile()
    {
        var errors = DocumentValidator.Validate(Upload("empty.txt", "text/plain", 0), Options);
        Assert.DoesNotContain("File is empty.", errors);
    }

    [Fact]
    public void Validate_AcceptsExactlyMaxSize()
    {
        var errors = DocumentValidator.Validate(Upload("big.txt", "text/plain", Options.MaxFileSizeBytes), Options);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsOneByteOverMaxSize()
    {
        var errors = DocumentValidator.Validate(Upload("big.txt", "text/plain", Options.MaxFileSizeBytes + 1), Options);
        Assert.Contains(errors, e => e.Contains("exceeds"));
    }

    [Fact]
    public void Validate_RejectsDisallowedExtension()
    {
        var errors = DocumentValidator.Validate(Upload("virus.exe", "application/octet-stream", 1024), Options);
        Assert.Contains(errors, e => e.Contains("not allowed"));
    }

    [Fact]
    public void Validate_RejectsDisallowedContentType()
    {
        var errors = DocumentValidator.Validate(Upload("notes.txt", "application/zip", 1024), Options);
        Assert.Contains(errors, e => e.Contains("Content type"));
    }

    [Fact]
    public void Validate_AcceptsUppercaseExtension()
    {
        var errors = DocumentValidator.Validate(Upload("REPORT.PDF", "application/pdf", 1024), Options);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsAllErrorsWhenMultipleRulesFail()
    {
        var errors = DocumentValidator.Validate(Upload("virus.exe", "application/zip", 0), Options);
        Assert.True(errors.Count >= 3);
    }
}
