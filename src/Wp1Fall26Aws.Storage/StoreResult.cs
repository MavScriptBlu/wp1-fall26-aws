namespace Wp1Fall26Aws.Storage;

// What comes back. A result object, not an exception, because rejection is
// an expected outcome here rather than an error.
public sealed record StoreResult(
    bool Success,
    string? ObjectKey,
    IReadOnlyList<string> Errors,
    IReadOnlyDictionary<string, string>? Metadata);
