namespace Wp1Fall26Aws.Storage;

// What the API hands down. No HttpContext, no IFormFile - those are web
// types and they do not belong below the boundary.
public sealed record DocumentUpload(
    string FileName,
    string ContentType,
    long ContentLength,
    Stream Content);
