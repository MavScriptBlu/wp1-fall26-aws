namespace Wp1Fall26Aws.Storage;

// Enforced before any S3 call. Every limit comes from S3StorageOptions so
// it stays configurable.
/// <summary>
/// A class that provides validation for document uploads.
/// </summary>
public static class DocumentValidator
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".pdf", ".png", ".jpg", ".jpeg"
    };

    /// <summary>
    /// The set of allowed content types.
    /// </summary>
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain", "application/pdf", "image/png", "image/jpeg"
    };

    /// <summary>
    /// Checks size, extension, and content type against the given options. Returns an
    /// empty list when the upload passes; otherwise one entry per failed rule.
    /// </summary>
    public static IReadOnlyList<string> Validate(DocumentUpload upload, S3StorageOptions options)
    {
        var errors = new List<string>();

        if (upload.ContentLength == 0)
        {
            errors.Add("File is empty.");
        }
        else if (upload.ContentLength > options.MaxFileSizeBytes)
        {
            errors.Add($"File exceeds the maximum size of {options.MaxFileSizeBytes} bytes.");
        }

        var ext = Path.GetExtension(upload.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            errors.Add($"Extension \x27{ext}\x27 is not allowed.");
        }

        if (string.IsNullOrEmpty(upload.ContentType) || !AllowedContentTypes.Contains(upload.ContentType))
        {
            errors.Add($"Content type \x27{upload.ContentType}\x27 is not allowed.");
        }

        return errors;
    }
}
